namespace CenterManagement.Application.DTOs.Student
{
    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
