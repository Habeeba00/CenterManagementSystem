using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class StudentProfile : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;  

        public string ParentPhone { get; set; } = string.Empty;

        public int GradeLevelId { get; set; }

        public GradeLevel GradeLevel { get; set; } = null!;

        public ICollection<Enrollment> Enrollments
        { get; set; } = new List<Enrollment>();

        public ICollection<StudentAttendance> Attendances
        { get; set; } = new List<StudentAttendance>();

        public ICollection<StudentCoursePayment> CoursePayments
        { get; set; } = new List<StudentCoursePayment>();

        public ICollection<SessionPayment> SessionPayments
        { get; set; } = new List<SessionPayment>();
    }
}