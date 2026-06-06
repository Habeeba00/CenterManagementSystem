using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Instructor
{
    public class UpdateInstructorViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required.")]
        public string Specialization { get; set; } = string.Empty;

        public int? SubjectId { get; set; }

        public IFormFile? Photo { get; set; }
        
        public string? CurrentImagePath { get; set; }

        [ValidateNever]
        public SelectList SubjectSelectList { get; set; } = null!;
    }
}
