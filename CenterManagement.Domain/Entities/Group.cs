using CenterManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CenterManagement.Domain.Entities
{
    public class Group : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public Course Course { get; set; } = null!;

        public int InstructorProfileId { get; set; }

        public InstructorProfile InstructorProfile { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<Session> Sessions { get; set; }
            = new List<Session>();

        public ICollection<Enrollment> Enrollments { get; set; }
            = new List<Enrollment>();
    }
}