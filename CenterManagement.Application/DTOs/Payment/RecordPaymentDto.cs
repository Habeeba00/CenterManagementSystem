using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Application.DTOs.Payment
{
    public class RecordPaymentDto
    {
        public int StudentCoursePaymentId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty; // "Cash" | "Card" | "Transfer"

        public string? Notes { get; set; }
    }
}
