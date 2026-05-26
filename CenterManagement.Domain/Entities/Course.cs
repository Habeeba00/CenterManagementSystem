using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int SubjectId { get; set; }

        public Subject Subject { get; set; } = new Subject();

        public int GradeLevelId { get; set; }

        public GradeLevel GradeLevel { get; set; } = new GradeLevel();

        public ICollection<Group> Groups
        { get; set; } = new List<Group>();

        public ICollection<StudentCoursePayment> StudentPayments
        { get; set; } = new List<StudentCoursePayment>();
    }
}