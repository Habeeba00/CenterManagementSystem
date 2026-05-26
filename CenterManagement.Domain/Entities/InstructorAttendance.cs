using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class InstructorAttendance : BaseEntity
    {
        public int InstructorProfileId { get; set; }

        public InstructorProfile InstructorProfile { get; set; } = null!;

        public DateTime ScanTime { get; set; }

        public bool IsPresent { get; set; }
    }
}