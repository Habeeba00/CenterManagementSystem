namespace CenterManagement.Application.DTOs.Analytics
{
    public class AnalyticsKpiDto
    {
        public int TotalStudents { get; set; }
        public int NewEnrollmentsThisMonth { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AttendanceRatePercent { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalGroups { get; set; }
    }
}
