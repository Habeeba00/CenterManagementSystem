using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CenterManagement.Web.Controllers
{
    /// <summary>
    /// Phase 1 stub only. Phase 6 replaces this entirely.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
