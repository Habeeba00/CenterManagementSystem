namespace CenterManagement.Application.DTOs.Payment
{
    public class SessionPaymentDto
    {
        public int SessionPaymentId { get; set; }
        public string SessionTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
