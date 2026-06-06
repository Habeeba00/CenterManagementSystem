namespace CenterManagement.Application.DTOs.Payment
{
    public class UnbilledEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
