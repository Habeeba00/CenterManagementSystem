namespace CenterManagement.Application.DTOs.Payment
{
    public class TransactionListItemDto
    {
        public int TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentImagePath { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public decimal RemainingAfterPayment { get; set; }
    }
}
