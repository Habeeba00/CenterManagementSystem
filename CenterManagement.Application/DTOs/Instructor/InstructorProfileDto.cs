using CenterManagement.Application.DTOs.Group;
using CenterManagement.Application.DTOs.Session;

namespace CenterManagement.Application.DTOs.Instructor
{
    public class InstructorProfileDto
    {
        public int InstructorProfileId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? SubjectName { get; set; }
        public int GroupCount { get; set; }
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public List<GroupListItemDto> Groups { get; set; } = new();
        public List<SessionListItemDto> UpcomingSessions { get; set; } = new();
    }
}
