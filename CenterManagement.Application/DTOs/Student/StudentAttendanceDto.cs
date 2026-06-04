namespace CenterManagement.Application.DTOs.Student
{
    public class StudentAttendanceDto
    {
        public int AttendanceId { get; set; }
        public DateTime SessionDate { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public DateTime ScanTime { get; set; }
        public bool IsPresent { get; set; }
        public bool IsLate { get; set; }
    }
}
