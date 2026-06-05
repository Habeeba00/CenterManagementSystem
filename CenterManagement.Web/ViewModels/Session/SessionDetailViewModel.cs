using CenterManagement.Application.DTOs.Session;

namespace CenterManagement.Web.ViewModels.Session
{
    public class SessionDetailViewModel
    {
        public SessionDetailDto SessionDetail { get; set; } = null!;
        public bool CanCancel { get; set; }
    }
}
