namespace CenterManagement.Application.DTOs.Course
{
    public class CreateCourseDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SubjectId { get; set; }
        public int GradeLevelId { get; set; }
    }
}
