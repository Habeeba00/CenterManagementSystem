namespace CenterManagement.Application.DTOs.Student
{
    public class StudentProfileDto
    {
        // All fields from StudentListItemDto
        public int StudentProfileId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string ParentPhone { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public decimal AttendanceRatePercent { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }

        // Profile-specific fields
        public string Email { get; set; } = string.Empty;
        public List<EnrollmentDto> Enrollments { get; set; } = new();
        public List<StudentCoursePaymentSummaryDto> CoursePayments { get; set; } = new();
        public List<StudentAttendanceDto> RecentAttendances { get; set; } = new(); // last 10
        public List<SessionScheduleDto> UpcomingSessions { get; set; } = new();   // Phase 3 populates; use empty list here
    }
}
