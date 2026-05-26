using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class StudentCoursePayment : BaseEntity
    {
        public int StudentProfileId { get; set; }

        public StudentProfile StudentProfile { get; set; } = null!;

        public int CourseId { get; set; }

        public Course Course { get; set; } = null!;

        public decimal RequiredAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public bool IsPaid { get; set; }

        // =========================
        // Navigation Properties
        // =========================

        public ICollection<PaymentTransaction> Transactions
        { get; set; } = new List<PaymentTransaction>();
    }
}