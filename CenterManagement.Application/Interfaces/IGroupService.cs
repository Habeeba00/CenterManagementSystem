using CenterManagement.Application.DTOs.Group;

namespace CenterManagement.Application.Interfaces
{
    public interface IGroupService
    {
        Task<int> CreateGroupAsync(CreateGroupDto dto, string adminId);
        Task<GroupDetailDto> GetGroupDetailAsync(int groupId);
        Task<List<GroupListItemDto>> GetAllGroupsAsync();
        Task<List<GroupListItemDto>> GetGroupsByInstructorAsync(int instructorProfileId);
        Task UpdateGroupAsync(int id, UpdateGroupDto dto, string adminId);
        Task ChangeInstructorAsync(int groupId, int newInstructorProfileId, string adminId);
        Task SoftDeleteAsync(int id, string adminId);
    }
}
