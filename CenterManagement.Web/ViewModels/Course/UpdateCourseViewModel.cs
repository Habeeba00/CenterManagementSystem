using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Course
{
    public class UpdateCourseViewModel
    {
        [Required(ErrorMessage = "Course name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Grade level is required.")]
        public int GradeLevelId { get; set; }

        public SelectList SubjectSelectList { get; set; } = null!;
        public SelectList GradeLevelSelectList { get; set; } = null!;
    }
}
