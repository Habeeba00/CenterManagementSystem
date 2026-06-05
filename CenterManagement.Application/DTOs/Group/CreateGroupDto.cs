namespace CenterManagement.Application.DTOs.Group
{
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int InstructorProfileId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
