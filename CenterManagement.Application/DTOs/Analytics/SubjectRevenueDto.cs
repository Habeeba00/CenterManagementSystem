namespace CenterManagement.Application.DTOs.Analytics
{
    public class SubjectRevenueDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal Percentage { get; set; }
    }
}
