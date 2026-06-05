namespace CenterManagement.Application.DTOs.Session
{
    public class CreateSessionDto
    {
        public int GroupId { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
