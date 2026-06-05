namespace CenterManagement.Application.DTOs.Analytics
{
    public class AttendanceTrendPointDto
    {
        public DateTime Date { get; set; }
        public decimal AttendanceRatePercent { get; set; }
        public int PresentCount { get; set; }
        public int TotalCount { get; set; }
    }
}
