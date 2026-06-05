using CenterManagement.Application.DTOs.Course;

namespace CenterManagement.Application.Interfaces
{
    public interface ICourseService
    {
        Task<int> CreateCourseAsync(CreateCourseDto dto, string adminId);
        Task<List<CourseDto>> GetAllCoursesAsync(int? gradeLevelId, int? subjectId);
        Task<CourseDto> GetCourseByIdAsync(int id);
        Task UpdateCourseAsync(int id, UpdateCourseDto dto, string adminId);
        Task SoftDeleteAsync(int id, string adminId);
    }
}
