using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public int EntityId { get; set; }

        public DateTime ActionDate { get; set; }
            = DateTime.UtcNow;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }
    }
}