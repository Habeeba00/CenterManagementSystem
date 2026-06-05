using CenterManagement.Application.DTOs.Group;
using CenterManagement.Application.DTOs.Session;
using CenterManagement.Application.DTOs.Student;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly CenterManagementDbContext _db;
        private readonly IAuditLogService _audit;

        public GroupService(CenterManagementDbContext db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        // =========================================
        // CreateGroupAsync
        // =========================================

        public async Task<int> CreateGroupAsync(CreateGroupDto dto, string adminId)
        {
            var group = new Group
            {
                Name = dto.Name,
                CourseId = dto.CourseId,
                InstructorProfileId = dto.InstructorProfileId,
                IsActive = dto.IsActive
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "GroupCreated",
                "Group",
                group.Id,
                null,
                JsonSerializer.Serialize(new { dto.Name, dto.CourseId, dto.InstructorProfileId }));

            return group.Id;
        }

        // =========================================
        // GetGroupDetailAsync
        // =========================================

        public async Task<GroupDetailDto> GetGroupDetailAsync(int groupId)
        {
            var group = await _db.Groups
                .Include(g => g.Course).ThenInclude(c => c.Subject)
                .Include(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(g => g.Enrollments).ThenInclude(e => e.StudentProfile).ThenInclude(sp => sp.User)
                .Include(g => g.Sessions.Where(s => !s.IsDeleted).OrderByDescending(s => s.SessionDate).Take(10))
                    .ThenInclude(s => s.Attendances)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId)
                ?? throw new KeyNotFoundException($"Group {groupId} not found.");

            return new GroupDetailDto
            {
                Id = group.Id,
                Name = group.Name,
                CourseName = group.Course.Name,
                SubjectName = group.Course.Subject.Name,
                InstructorName = group.InstructorProfile?.User?.FullName ?? "N/A",
                InstructorProfileId = group.InstructorProfileId,
                EnrollmentCount = group.Enrollments.Count(e => e.IsActive),
                IsActive = group.IsActive,
                RecentSessions = group.Sessions.Select(s => new SessionListItemDto
                {
                    Id = s.Id,
                    SessionDate = s.SessionDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    GroupName = group.Name,
                    CourseName = group.Course.Name,
                    SubjectName = group.Course.Subject.Name,
                    GradeLevelName = group.Course.GradeLevel.Name,
                    InstructorName = group.InstructorProfile?.User?.FullName ?? "N/A",
                    IsCanceled = s.IsCanceled,
                    CancelReason = s.CancelReason,
                    AttendanceCount = s.Attendances.Count(a => a.IsPresent)
                }).ToList(),
                Enrollments = group.Enrollments.Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.Id,
                    GroupId = e.GroupId,
                    GroupName = group.Name,
                    CourseName = group.Course.Name,
                    SubjectName = group.Course.Subject.Name,
                    InstructorName = group.InstructorProfile?.User?.FullName ?? "N/A",
                    IsActive = e.IsActive,
                    EnrollmentDate = e.EnrollmentDate
                }).ToList()
            };
        }

        // =========================================
        // GetAllGroupsAsync
        // =========================================

        public async Task<List<GroupListItemDto>> GetAllGroupsAsync()
        {
            var groups = await _db.Groups
                .Include(g => g.Course).ThenInclude(c => c.Subject)
                .Include(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(g => g.Enrollments)
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();

            return groups.Select(MapToListItem).ToList();
        }

        // =========================================
        // GetGroupsByInstructorAsync
        // =========================================

        public async Task<List<GroupListItemDto>> GetGroupsByInstructorAsync(int instructorProfileId)
        {
            var groups = await _db.Groups
                .Include(g => g.Course).ThenInclude(c => c.Subject)
                .Include(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(g => g.Enrollments)
                .AsNoTracking()
                .Where(g => g.InstructorProfileId == instructorProfileId)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return groups.Select(MapToListItem).ToList();
        }

        // =========================================
        // UpdateGroupAsync
        // =========================================

        public async Task UpdateGroupAsync(int id, UpdateGroupDto dto, string adminId)
        {
            var group = await _db.Groups
                .FirstOrDefaultAsync(g => g.Id == id)
                ?? throw new KeyNotFoundException($"Group {id} not found.");

            var oldValues = JsonSerializer.Serialize(new { group.Name, group.IsActive });

            group.Name = dto.Name;
            group.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "GroupUpdated", "Group", id, oldValues,
                JsonSerializer.Serialize(new { dto.Name, dto.IsActive }));
        }

        // =========================================
        // ChangeInstructorAsync
        // =========================================

        public async Task ChangeInstructorAsync(int groupId, int newInstructorProfileId, string adminId)
        {
            var group = await _db.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId)
                ?? throw new KeyNotFoundException($"Group {groupId} not found.");

            var oldId = group.InstructorProfileId;
            group.InstructorProfileId = newInstructorProfileId;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "InstructorChanged",
                "Group",
                groupId,
                oldId.ToString(),
                newInstructorProfileId.ToString());
        }

        // =========================================
        // SoftDeleteAsync
        // =========================================

        public async Task SoftDeleteAsync(int id, string adminId)
        {
            var group = await _db.Groups
                .Include(g => g.Enrollments)
                .FirstOrDefaultAsync(g => g.Id == id)
                ?? throw new KeyNotFoundException($"Group {id} not found.");

            // Check no active Enrollments before soft-deleting
            if (group.Enrollments.Any(e => e.IsActive))
                throw new InvalidOperationException(
                    "Cannot delete group with active enrollments. Remove students first.");

            group.IsDeleted = true;
            group.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "GroupSoftDeleted", "Group", id, null, null);
        }

        // =========================================
        // Private Helpers
        // =========================================

        private static GroupListItemDto MapToListItem(Group g)
        {
            return new GroupListItemDto
            {
                Id = g.Id,
                Name = g.Name,
                CourseName = g.Course.Name,
                SubjectName = g.Course.Subject.Name,
                InstructorName = g.InstructorProfile?.User?.FullName ?? "N/A",
                InstructorProfileId = g.InstructorProfileId,
                EnrollmentCount = g.Enrollments.Count(e => e.IsActive),
                IsActive = g.IsActive
            };
        }
    }
}
