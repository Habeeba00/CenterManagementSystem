namespace CenterManagement.Application.DTOs.Analytics
{
    public class TopTeacherDto
    {
        public int InstructorProfileId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public decimal AttendanceRatePercent { get; set; }
        public int TotalSessionsConducted { get; set; }
        public int GroupCount { get; set; }
    }
}
