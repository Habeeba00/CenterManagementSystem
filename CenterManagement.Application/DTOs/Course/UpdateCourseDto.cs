namespace CenterManagement.Application.DTOs.Course
{
    public class UpdateCourseDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SubjectId { get; set; }
        public int GradeLevelId { get; set; }
    }
}
