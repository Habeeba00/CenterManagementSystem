namespace CenterManagement.Application.DTOs.Student
{
    public class StudentCoursePaymentSummaryDto
    {
        public int StudentCoursePaymentId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal RequiredAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsPaid { get; set; }
    }
}
