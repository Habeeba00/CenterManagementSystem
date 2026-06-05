using CenterManagement.Application.DTOs.Payment;

namespace CenterManagement.Web.ViewModels.Payment
{
    public class PaymentIndexViewModel
    {
        public PaymentKpiDto Kpis { get; set; } = new();
        public TransactionFilter CurrentFilter { get; set; } = new();
    }
}
