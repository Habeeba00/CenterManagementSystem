using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class InstructorProfile : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public string Specialization { get; set; } = string.Empty;

        public int? SubjectId { get; set; }

        public Subject? Subject { get; set; }

        public ICollection<Group> Groups
        { get; set; } = new List<Group>();

        public ICollection<InstructorAttendance> Attendances
        { get; set; } = new List<InstructorAttendance>();
    }
}