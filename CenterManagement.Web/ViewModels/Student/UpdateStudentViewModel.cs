using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Student
{
    public class UpdateStudentViewModel
    {
        public int StudentProfileId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Parent phone is required.")]
        public string ParentPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade level is required.")]
        public int GradeLevelId { get; set; }

        public IFormFile? Photo { get; set; }

        public string? CurrentImagePath { get; set; }

        [ValidateNever]
        public SelectList GradeLevelSelectList { get; set; } = null!;
    }
}
