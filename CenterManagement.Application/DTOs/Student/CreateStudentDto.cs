using Microsoft.AspNetCore.Http;

namespace CenterManagement.Application.DTOs.Student
{
    public class CreateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string ParentPhone { get; set; } = string.Empty;
        public int GradeLevelId { get; set; }
        public List<int> GroupIds { get; set; } = new();
        public IFormFile? Photo { get; set; }
    }
}
