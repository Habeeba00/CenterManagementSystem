using CenterManagement.Application.DTOs.Instructor;

namespace CenterManagement.Application.Interfaces
{
    public interface IInstructorService
    {
        Task<int> CreateInstructorAsync(CreateInstructorDto dto, string adminId);
        Task<InstructorProfileDto> GetInstructorProfileAsync(int id);
        Task<List<InstructorListItemDto>> GetAllInstructorsAsync();
        Task UpdateInstructorAsync(int id, UpdateInstructorDto dto, string adminId);
        Task SoftDeleteAsync(int id, string adminId);
    }
}
