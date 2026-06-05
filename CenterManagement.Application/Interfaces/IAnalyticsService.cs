using CenterManagement.Application.DTOs.Analytics;

namespace CenterManagement.Application.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsKpiDto> GetAnalyticsKpisAsync();
        Task<List<AttendanceTrendPointDto>> GetAttendanceTrend30DaysAsync();
        Task<List<GradeDistributionDto>> GetStudentDistributionByGradeAsync();
        Task<List<SubjectRevenueDto>> GetRevenueBySubjectAsync();
        Task<List<TopTeacherDto>> GetTopTeachersAsync(int topN = 5);
        Task<List<SmartInsightDto>> GetSmartInsightsAsync();
    }
}
