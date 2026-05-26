using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class SessionPayment : BaseEntity
    {
        public int StudentProfileId { get; set; }

        public StudentProfile StudentProfile { get; set; } = null!;

        public int SessionId { get; set; }

        public Session Session { get; set; } = null!;

        public decimal Amount { get; set; }

        // =========================
        // Admin
        // =========================

        public string AdminId { get; set; } = string.Empty;

        public ApplicationUser Admin { get; set; } = null!;

        public DateTime PaymentDate { get; set; }
            = DateTime.UtcNow;
    }
}