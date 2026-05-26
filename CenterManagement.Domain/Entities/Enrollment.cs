using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public int StudentProfileId { get; set; }

        public StudentProfile StudentProfile { get; set; } = null!;

        public int GroupId { get; set; }

        public Group Group { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; }
            = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}