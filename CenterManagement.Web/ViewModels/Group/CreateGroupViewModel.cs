using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Group
{
    public class CreateGroupViewModel
    {
        [Required(ErrorMessage = "Group name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Instructor is required.")]
        public int InstructorProfileId { get; set; }
        
        public bool IsActive { get; set; } = true;

        [ValidateNever]
        public SelectList CourseSelectList { get; set; } = null!;
        [ValidateNever]
        public SelectList InstructorSelectList { get; set; } = null!;
    }
}
