namespace CenterManagement.Application.DTOs.Dashboard
{
    public class ActiveSessionDto
    {
        public int SessionId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TotalEnrolled { get; set; }
        public int PresentCount { get; set; }
    }
}
