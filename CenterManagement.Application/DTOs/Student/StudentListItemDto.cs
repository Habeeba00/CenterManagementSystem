namespace CenterManagement.Application.DTOs.Student
{
    public class StudentListItemDto
    {
        public int StudentProfileId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string ParentPhone { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public decimal AttendanceRatePercent { get; set; }
        public string PaymentStatus { get; set; } = string.Empty; // Paid / Partial / Unpaid
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
