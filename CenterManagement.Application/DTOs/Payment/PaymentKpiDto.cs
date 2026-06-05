namespace CenterManagement.Application.DTOs.Payment
{
    public class PaymentKpiDto
    {
        public decimal TotalDueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal TotalRevenueAllTime { get; set; }
        public int OutstandingStudentsCount { get; set; }
        public decimal CollectionRatePercent { get; set; }
        public int PaidStudentsCount { get; set; }
    }
}
