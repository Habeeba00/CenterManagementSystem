using CenterManagement.Application.DTOs.Course;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly CenterManagementDbContext _db;
        private readonly IAuditLogService _audit;

        public CourseService(CenterManagementDbContext db, IAuditLogService audit)
        {
            _db = db;
            _audit = audit;
        }

        // =========================================
        // CreateCourseAsync
        // =========================================

        public async Task<int> CreateCourseAsync(CreateCourseDto dto, string adminId)
        {
            // Validate no existing non-deleted course with same Name + GradeLevelId + SubjectId
            var duplicate = await _db.Courses
                .AnyAsync(c =>
                    c.Name == dto.Name &&
                    c.GradeLevelId == dto.GradeLevelId &&
                    c.SubjectId == dto.SubjectId);

            if (duplicate)
                throw new InvalidOperationException(
                    "A course with the same name, grade level, and subject already exists.");

            var course = new Course
            {
                Name = dto.Name,
                Price = dto.Price,
                SubjectId = dto.SubjectId,
                GradeLevelId = dto.GradeLevelId
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                adminId,
                "CourseCreated",
                "Course",
                course.Id,
                null,
                JsonSerializer.Serialize(new { dto.Name, dto.Price, dto.SubjectId, dto.GradeLevelId }));

            return course.Id;
        }

        // =========================================
        // GetAllCoursesAsync
        // =========================================

        public async Task<List<CourseDto>> GetAllCoursesAsync(int? gradeLevelId, int? subjectId)
        {
            var query = _db.Courses
                .Include(c => c.Subject)
                .Include(c => c.GradeLevel)
                .Include(c => c.Groups)
                .AsNoTracking()
                .AsQueryable();

            if (gradeLevelId.HasValue)
                query = query.Where(c => c.GradeLevelId == gradeLevelId);

            if (subjectId.HasValue)
                query = query.Where(c => c.SubjectId == subjectId);

            var courses = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            return courses.Select(c => new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Price = c.Price,
                SubjectName = c.Subject.Name,
                GradeLevelName = c.GradeLevel.Name,
                GroupCount = c.Groups.Count(g => !g.IsDeleted)
            }).ToList();
        }

        // =========================================
        // GetCourseByIdAsync
        // =========================================

        public async Task<CourseDto> GetCourseByIdAsync(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Subject)
                .Include(c => c.GradeLevel)
                .Include(c => c.Groups)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Course {id} not found.");

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Price = course.Price,
                SubjectName = course.Subject.Name,
                GradeLevelName = course.GradeLevel.Name,
                GroupCount = course.Groups.Count(g => !g.IsDeleted)
            };
        }

        // =========================================
        // UpdateCourseAsync
        // =========================================

        public async Task UpdateCourseAsync(int id, UpdateCourseDto dto, string adminId)
        {
            var course = await _db.Courses
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Course {id} not found.");

            var oldValues = JsonSerializer.Serialize(new
            {
                course.Name,
                course.Price,
                course.SubjectId,
                course.GradeLevelId
            });

            course.Name = dto.Name;
            course.Price = dto.Price;
            course.SubjectId = dto.SubjectId;
            course.GradeLevelId = dto.GradeLevelId;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "CourseUpdated", "Course", id, oldValues,
                JsonSerializer.Serialize(new { dto.Name, dto.Price, dto.SubjectId, dto.GradeLevelId }));
        }

        // =========================================
        // SoftDeleteAsync
        // =========================================

        public async Task SoftDeleteAsync(int id, string adminId)
        {
            var course = await _db.Courses
                .Include(c => c.Groups)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException($"Course {id} not found.");

            // Check no active Groups linked to this course before deleting
            if (course.Groups.Any(g => !g.IsDeleted))
                throw new InvalidOperationException(
                    "Cannot delete course with active groups. Remove or delete groups first.");

            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, "CourseSoftDeleted", "Course", id, null, null);
        }
    }
}
