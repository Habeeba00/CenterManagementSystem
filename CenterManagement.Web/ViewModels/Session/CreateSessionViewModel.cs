using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Session
{
    public class CreateSessionViewModel
    {
        [Required(ErrorMessage = "Group is required.")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Session date is required.")]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public SelectList GroupSelectList { get; set; } = null!;
    }
}
