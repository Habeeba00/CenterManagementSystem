using Microsoft.AspNetCore.Http;

namespace CenterManagement.Application.DTOs.Student
{
    public class UpdateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string ParentPhone { get; set; } = string.Empty;
        public int GradeLevelId { get; set; }
        public IFormFile? Photo { get; set; }
        // Email is NOT editable after account creation
    }
}
