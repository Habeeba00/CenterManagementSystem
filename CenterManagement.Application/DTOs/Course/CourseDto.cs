namespace CenterManagement.Application.DTOs.Course
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string GradeLevelName { get; set; } = string.Empty;
        public int GroupCount { get; set; }
    }
}
