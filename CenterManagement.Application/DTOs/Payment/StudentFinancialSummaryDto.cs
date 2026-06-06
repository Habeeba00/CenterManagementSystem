using CenterManagement.Application.DTOs.Student;

namespace CenterManagement.Application.DTOs.Payment
{
    public class StudentFinancialSummaryDto
    {
        public int StudentProfileId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal TotalRequired { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public bool IsFullyPaid { get; set; }
        public List<StudentCoursePaymentSummaryDto> CoursePayments { get; set; } = new();
        public List<SessionPaymentDto> SessionPayments { get; set; } = new();
        public List<UnbilledEnrollmentDto> UnbilledEnrollments { get; set; } = new();
    }
}
