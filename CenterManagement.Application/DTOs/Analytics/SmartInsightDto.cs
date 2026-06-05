namespace CenterManagement.Application.DTOs.Analytics
{
    public class SmartInsightDto
    {
        public string Category { get; set; } = string.Empty;   // "Low Attendance" | "High Cancellation Rate"
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;   // "Warning" | "Critical"
        public int AffectedCount { get; set; }
    }
}
