using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class GradeLevel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // =========================
        // Navigation Properties
        // =========================

        public ICollection<StudentProfile> Students
        { get; set; } = new List<StudentProfile>();

        public ICollection<Course> Courses
        { get; set; } = new List<Course>();
    }
}