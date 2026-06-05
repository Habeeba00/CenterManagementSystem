using CenterManagement.Application.DTOs.Analytics;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly CenterManagementDbContext _db;

        public AnalyticsService(CenterManagementDbContext db)
        {
            _db = db;
        }

        public async Task<AnalyticsKpiDto> GetAnalyticsKpisAsync()
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var totalStudents = await _db.StudentProfiles.CountAsync();

            var newEnrollments = await _db.Enrollments
                .CountAsync(e => e.CreatedAt >= monthStart);

            var monthlyRevenue = await _db.PaymentTransactions
                .Where(t => t.PaymentDate >= monthStart)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalAttendance = await _db.StudentAttendances
                .CountAsync(a => a.CreatedAt >= thirtyDaysAgo);
            var presentAttendance = await _db.StudentAttendances
                .CountAsync(a => a.CreatedAt >= thirtyDaysAgo && a.IsPresent);
            var attendanceRate = totalAttendance > 0
                ? (decimal)presentAttendance / totalAttendance * 100 : 0;

            var totalInstructors = await _db.InstructorProfiles.CountAsync();
            var totalGroups = await _db.Groups.CountAsync();

            return new AnalyticsKpiDto
            {
                TotalStudents = totalStudents,
                NewEnrollmentsThisMonth = newEnrollments,
                MonthlyRevenue = monthlyRevenue,
                AttendanceRatePercent = attendanceRate,
                TotalInstructors = totalInstructors,
                TotalGroups = totalGroups
            };
        }

        public async Task<List<AttendanceTrendPointDto>> GetAttendanceTrend30DaysAsync()
        {
            var from = DateTime.UtcNow.Date.AddDays(-29);

            var raw = await _db.StudentAttendances
                .Where(a => a.Session.SessionDate >= from)
                .GroupBy(a => a.Session.SessionDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    PresentCount = g.Count(a => a.IsPresent),
                    TotalCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return raw.Select(x => new AttendanceTrendPointDto
            {
                Date = x.Date,
                PresentCount = x.PresentCount,
                TotalCount = x.TotalCount,
                AttendanceRatePercent = x.TotalCount > 0
                    ? (decimal)x.PresentCount / x.TotalCount * 100 : 0
            }).ToList();
        }

        public async Task<List<GradeDistributionDto>> GetStudentDistributionByGradeAsync()
        {
            var totalStudents = await _db.StudentProfiles.CountAsync();

            if (totalStudents == 0)
                return new List<GradeDistributionDto>();

            return await _db.StudentProfiles
                .GroupBy(sp => sp.GradeLevel.Name)
                .Select(g => new GradeDistributionDto
                {
                    GradeLevelName = g.Key,
                    StudentCount = g.Count(),
                    Percentage = (decimal)g.Count() / totalStudents * 100
                })
                .OrderByDescending(x => x.StudentCount)
                .ToListAsync();
        }

        public async Task<List<SubjectRevenueDto>> GetRevenueBySubjectAsync()
        {
            var totalRevenue = await _db.PaymentTransactions
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            if (totalRevenue == 0)
                return new List<SubjectRevenueDto>();

            return await _db.PaymentTransactions
                .GroupBy(t => t.StudentCoursePayment.Course.Subject.Name)
                .Select(g => new SubjectRevenueDto
                {
                    SubjectName = g.Key,
                    TotalRevenue = g.Sum(t => t.Amount),
                    Percentage = totalRevenue > 0
                        ? g.Sum(t => t.Amount) / totalRevenue * 100
                        : 0
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();
        }

        public async Task<List<TopTeacherDto>> GetTopTeachersAsync(int topN = 5)
        {
            return await _db.InstructorProfiles
                .Select(ip => new TopTeacherDto
                {
                    InstructorProfileId = ip.Id,
                    InstructorName = ip.User.FullName,
                    ImagePath = ip.User.ImagePath,
                    SubjectName = ip.Subject != null ? ip.Subject.Name : "—",
                    GroupCount = ip.Groups.Count(g => !g.IsDeleted),
                    TotalSessionsConducted = ip.Groups
                        .SelectMany(g => g.Sessions)
                        .Count(s => !s.IsCanceled),
                    AttendanceRatePercent = ip.Groups
                        .SelectMany(g => g.Sessions)
                        .SelectMany(s => s.Attendances)
                        .Any()
                        ? (decimal)(ip.Groups.SelectMany(g => g.Sessions)
                            .SelectMany(s => s.Attendances)
                            .Average(a => a.IsPresent ? 1.0 : 0.0) * 100)
                        : 0
                })
                .OrderByDescending(x => x.AttendanceRatePercent)
                .Take(topN)
                .ToListAsync();
        }

        public async Task<List<SmartInsightDto>> GetSmartInsightsAsync()
        {
            var insights = new List<SmartInsightDto>();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // Query 1 — Low attendance students
            var studentsWithLowAttendance = await _db.StudentProfiles
                .Where(sp => _db.StudentAttendances
                    .Where(a => a.StudentProfileId == sp.Id && a.CreatedAt >= thirtyDaysAgo)
                    .Any())
                .Select(sp => new
                {
                    sp.Id,
                    Total = _db.StudentAttendances.Count(a => a.StudentProfileId == sp.Id && a.CreatedAt >= thirtyDaysAgo),
                    Present = _db.StudentAttendances.Count(a => a.StudentProfileId == sp.Id && a.IsPresent && a.CreatedAt >= thirtyDaysAgo)
                })
                .Where(x => x.Total > 0 && (decimal)x.Present / x.Total < 0.8m)
                .CountAsync();

            if (studentsWithLowAttendance > 0)
                insights.Add(new SmartInsightDto
                {
                    Category = "Low Attendance",
                    Description = $"{studentsWithLowAttendance} student(s) have attended less than 80% of sessions in the last 30 days.",
                    Severity = studentsWithLowAttendance > 10 ? "Critical" : "Warning",
                    AffectedCount = studentsWithLowAttendance
                });

            // Query 2 — High cancellation groups
            var highCancellationGroups = await _db.Sessions
                .Where(s => s.IsCanceled && s.SessionDate >= thirtyDaysAgo)
                .GroupBy(s => s.GroupId)
                .Where(g => g.Count() > 2)
                .CountAsync();

            if (highCancellationGroups > 0)
                insights.Add(new SmartInsightDto
                {
                    Category = "High Cancellation Rate",
                    Description = $"{highCancellationGroups} group(s) have had more than 2 canceled sessions in the last 30 days.",
                    Severity = "Warning",
                    AffectedCount = highCancellationGroups
                });

            return insights;
        }
    }
}
