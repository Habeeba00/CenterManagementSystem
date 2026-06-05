namespace CenterManagement.Application.DTOs.Session
{
    public class SessionScheduleDto
    {
        public int SessionId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCanceled { get; set; }
        public int EnrolledCount { get; set; }
    }
}
