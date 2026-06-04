using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Student
{
    public class TransferStudentViewModel
    {
        public int StudentProfileId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int FromGroupId { get; set; }
        public string FromGroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a target group.")]
        public int ToGroupId { get; set; }

        public SelectList AvailableGroupsSelectList { get; set; } = null!;
    }
}
