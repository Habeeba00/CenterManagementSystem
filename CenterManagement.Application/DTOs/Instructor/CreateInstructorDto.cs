using Microsoft.AspNetCore.Http;

namespace CenterManagement.Application.DTOs.Instructor
{
    public class CreateInstructorDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int? SubjectId { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
