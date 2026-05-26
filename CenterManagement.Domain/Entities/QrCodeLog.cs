using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class QrCodeLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public DateTime ScanTime { get; set; }

        public string QrCode { get; set; } = string.Empty;
    }
}