using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Application.DTOs.Payment
{
    public class CreateSessionPaymentDto
    {
        public int StudentProfileId { get; set; }

        public int SessionId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}
