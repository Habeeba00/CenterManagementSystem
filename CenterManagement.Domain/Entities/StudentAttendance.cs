using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class StudentAttendance : BaseEntity
    {
        public int StudentProfileId { get; set; }

        public StudentProfile StudentProfile { get; set; } = null!;

        public int SessionId { get; set; }

        public Session Session { get; set; } = null!;

        public DateTime ScanTime { get; set; }

        public bool IsPresent { get; set; }

        public bool IsLate { get; set; }
    }
}