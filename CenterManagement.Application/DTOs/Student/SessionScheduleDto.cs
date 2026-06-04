namespace CenterManagement.Application.DTOs.Student
{
    /// <summary>
    /// Placeholder DTO for upcoming session schedule.
    /// Phase 3 populates this; Phase 2 uses an empty list.
    /// </summary>
    public class SessionScheduleDto
    {
        public int SessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
    }
}
