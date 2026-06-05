namespace CenterManagement.Application.DTOs.Analytics
{
    public class GradeDistributionDto
    {
        public string GradeLevelName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal Percentage { get; set; }
    }
}
