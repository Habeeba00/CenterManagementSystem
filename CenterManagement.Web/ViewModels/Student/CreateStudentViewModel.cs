using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Student
{
    public class CreateStudentViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Parent phone is required.")]
        public string ParentPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade level is required.")]
        public int GradeLevelId { get; set; }

        public List<int> GroupIds { get; set; } = new();

        public IFormFile? Photo { get; set; }

        public SelectList GradeLevelSelectList { get; set; } = null!;
        public SelectList GroupSelectList { get; set; } = null!;
    }
}
