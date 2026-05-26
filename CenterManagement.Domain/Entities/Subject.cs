using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CenterManagement.Domain.Common;

namespace CenterManagement.Domain.Entities
{
    public class Subject : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Course> Courses
        { get; set; } = new List<Course>();

        public ICollection<InstructorProfile> Instructors
        { get; set; } = new List<InstructorProfile>();
    }
}