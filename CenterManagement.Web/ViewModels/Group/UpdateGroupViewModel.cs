using System.ComponentModel.DataAnnotations;

namespace CenterManagement.Web.ViewModels.Group
{
    public class UpdateGroupViewModel
    {
        [Required(ErrorMessage = "Group name is required.")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
