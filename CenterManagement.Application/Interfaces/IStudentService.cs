using CenterManagement.Application.DTOs.Common;
using CenterManagement.Application.DTOs.Student;

namespace CenterManagement.Application.Interfaces
{
    public interface IStudentService
    {
        Task<int> CreateStudentAsync(CreateStudentDto dto, string adminId);
        Task<StudentProfileDto> GetStudentProfileAsync(int studentProfileId);
        Task<PagedResult<StudentListItemDto>> GetStudentListAsync(StudentListFilter filter);
        Task UpdateStudentAsync(int id, UpdateStudentDto dto, string adminId);
        Task SoftDeleteAsync(int id, string adminId);
        Task ToggleActiveAsync(int id, string adminId);
        Task TransferStudentAsync(int studentProfileId, int fromGroupId, int toGroupId, string adminId);
        Task AddToGroupAsync(int studentProfileId, int groupId, string adminId);
        Task RemoveFromGroupAsync(int studentProfileId, int groupId, string adminId);
        Task<List<StudentListItemDto>> SearchStudentsAsync(string query, int maxResults = 10);
    }
}
