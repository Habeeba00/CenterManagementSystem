namespace CenterManagement.Application.DTOs.Payment
{
    public class TransactionFilter
    {
        public string? StudentName { get; set; }
        public string? PaymentStatus { get; set; } // "Paid" | "Pending" | null = all
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
