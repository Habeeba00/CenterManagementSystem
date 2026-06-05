namespace CenterManagement.Application.DTOs.Group
{
    public class GroupListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int InstructorProfileId { get; set; }
        public int EnrollmentCount { get; set; }
        public bool IsActive { get; set; }
    }
}
