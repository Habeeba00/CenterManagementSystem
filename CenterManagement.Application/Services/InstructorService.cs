using CenterManagement.Application.DTOs.Group;
using CenterManagement.Application.DTOs.Instructor;
using CenterManagement.Application.DTOs.Session;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly CenterManagementDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _audit;
        private readonly IFileUploadService _fileUpload;

        public InstructorService(
            CenterManagementDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditLogService audit,
            IFileUploadService fileUpload)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _audit = audit;
            _fileUpload = fileUpload;
        }

        // =========================================
        // CreateInstructorAsync
        // =========================================

        public async Task<int> CreateInstructorAsync(CreateInstructorDto dto, string adminId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            ApplicationUser? user = null;

            try
            {
                // 1. Create ApplicationUser
                var tempPassword = "Instructor@" + Guid.NewGuid().ToString()[..6];

                user = new ApplicationUser
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                // 2. Assign Instructor role
                await _userManager.AddToRoleAsync(user, "Instructor");

                // 3. Upload photo if provided
                if (dto.Photo != null)
                {
                    user.ImagePath = await _fileUpload.UploadPhotoAsync(dto.Photo, "instructors");
                    await _userManager.UpdateAsync(user);
                }

                // 4. Create InstructorProfile
                var profile = new InstructorProfile
                {
                    UserId = user.Id,
                    Specialization = dto.Specialization,
                    SubjectId = dto.SubjectId
                };
                _db.InstructorProfiles.Add(profile);
                await _db.SaveChangesAsync();

                // 5. Commit transaction
                await transaction.CommitAsync();

                // 6. Audit
                await _audit.LogAsync(
                    adminId,
                    "InstructorCreated",
                    "InstructorProfile",
                    profile.Id,
                    null,
                    JsonSerializer.Serialize(new { dto.FullName, dto.Email, dto.Specialization, dto.SubjectId }));

                return profile.Id;
            }
            catch
            {
                await transaction.RollbackAsync();

                // Clean up orphaned user if it was created
                if (user != null && !string.IsNullOrEmpty(user.Id))
                {
                    var existingUser = await _userManager.FindByIdAsync(user.Id);
                    if (existingUser != null)
                    {
                        await _userManager.DeleteAsync(existingUser);
                    }
                }

                throw;
            }
        }

        // =========================================
        // GetInstructorProfileAsync
        // =========================================

        public async Task<InstructorProfileDto> GetInstructorProfileAsync(int id)
        {
            var instructor = await _db.InstructorProfiles
                .Include(i => i.User)
                .Include(i => i.Subject)
                .Include(i => i.Groups).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(i => i.Groups).ThenInclude(g => g.Enrollments)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException($"Instructor profile {id} not found.");

            // Get upcoming sessions for this instructor's groups
            var groupIds = instructor.Groups.Where(g => !g.IsDeleted).Select(g => g.Id).ToList();
            var upcomingSessions = await _db.Sessions
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.Subject)
                .Include(s => s.Group).ThenInclude(g => g.Course).ThenInclude(c => c.GradeLevel)
                .Include(s => s.Group).ThenInclude(g => g.InstructorProfile).ThenInclude(ip => ip.User)
                .Include(s => s.Attendances)
                .AsNoTracking()
                .Where(s => groupIds.Contains(s.GroupId) &&
                            s.SessionDate >= DateTime.UtcNow.Date &&
                            !s.IsCanceled)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .Take(10)
                .ToListAsync();

            return new InstructorProfileDto
            {
                InstructorProfileId = instructor.Id,
                UserId = instructor.UserId,
                FullName = instructor.User.FullName,
                Specialization = instructor.Specialization,
                SubjectName = instructor.Subject?.Name,
                GroupCount = instructor.Groups.Count(g => !g.IsDeleted),
                IsActive = instructor.User.IsActive,
                ImagePath = instructor.User.ImagePath,
                Groups = instructor.Groups.Where(g => !g.IsDeleted).Select(g => new GroupListItemDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CourseName = g.Course.Name,
                    SubjectName = g.Course.Subject.Name,
                    InstructorName = instructor.User.FullName,
                    InstructorProfileId = instructor.Id,
                    EnrollmentCount = g.Enrollments.Count(e => e.IsActive),
                    IsActive = g.IsActive
                }).ToList(),
                UpcomingSessions = upcomingSessions.Select(s => new SessionListItemDto
                {
                    Id = s.Id,
                    SessionDate = s.SessionDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    GroupName = s.Group.Name,
                    CourseName = s.Group.Course.Name,
                    SubjectName = s.Group.Course.Subject.Name,
                    GradeLevelName = s.Group.Course.GradeLevel.Name,
                    InstructorName = instructor.User.FullName,
                    IsCanceled = s.IsCanceled,
                    CancelReason = s.CancelReason,
                    AttendanceCount = s.Attendances.Count(a => a.IsPresent)
                }).ToList()
            };
        }

        // =========================================
        // GetAllInstructorsAsync
        // =========================================

        public async Task<List<InstructorListItemDto>> GetAllInstructorsAsync()
        {
            var instructors = await _db.InstructorProfiles
                .Include(i => i.User)
                .Include(i => i.Subject)
                .Include(i => i.Groups)
                .AsNoTracking()
                .OrderBy(i => i.User.FullName)
                .ToListAsync();

            return instructors.Select(i => new InstructorListItemDto
            {
                InstructorProfileId = i.Id,
                UserId = i.UserId,
                FullName = i.User.FullName,
                Specialization = i.Specialization,
                SubjectName = i.Subject?.Name,
                GroupCount = i.Groups.Count(g => !g.IsDeleted),
                IsActive = i.User.IsActive,
                ImagePath = i.User.ImagePath
            }).ToList();
        }

        // =========================================
        // UpdateInstructorAsync
        // =========================================

        public async Task UpdateInstructorAsync(int id, UpdateInstructorDto dto, string adminId)
        {
            var instructor = await _db.InstructorProfiles
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException($"Instructor profile {id} not found.");

            var oldValues = JsonSerializer.Serialize(new
            {
                instructor.User.FullName,
                instructor.Specialization,
                instructor.SubjectId
            });

            instructor.User.FullName = dto.FullName;
            instructor.Specialization = dto.Specialization;
            instructor.SubjectId = dto.SubjectId;

            // Handle photo upload
            if (dto.Photo != null)
            {
                if (!string.IsNullOrEmpty(instructor.User.ImagePath))
                {
                    _fileUpload.DeleteFile(instructor.User.ImagePath);
                }
                instructor.User.ImagePath = await _fileUpload.UploadPhotoAsync(dto.Photo, "instructors");
            }

            await _userManager.UpdateAsync(instructor.User);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "InstructorUpdated", "InstructorProfile", id, oldValues,
                JsonSerializer.Serialize(new { dto.FullName, dto.Specialization, dto.SubjectId }));
        }

        // =========================================
        // SoftDeleteAsync
        // =========================================

        public async Task SoftDeleteAsync(int id, string adminId)
        {
            var instructor = await _db.InstructorProfiles
                .Include(i => i.Groups)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException($"Instructor profile {id} not found.");

            // Check no active Groups linked to this instructor
            if (instructor.Groups.Any(g => !g.IsDeleted))
                throw new InvalidOperationException(
                    "Cannot delete instructor with active groups. Reassign or remove groups first.");

            instructor.IsDeleted = true;
            instructor.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "InstructorSoftDeleted", "InstructorProfile", id, null, null);
        }
    }
}
