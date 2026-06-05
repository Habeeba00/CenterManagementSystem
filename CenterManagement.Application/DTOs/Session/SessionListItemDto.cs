namespace CenterManagement.Application.DTOs.Session
{
    public class SessionListItemDto
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public bool IsCanceled { get; set; }
        public string? CancelReason { get; set; }
        public int AttendanceCount { get; set; }
    }
}
