using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CenterManagement.Web.Controllers
{
    /// <summary>
    /// Phase 6 — Receptionist Dashboard with real-time KPIs and active sessions.
    /// </summary>
    [Authorize(Roles = "Admin,Instructor")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Kpis()
        {
            var kpis = await _dashboardService.GetReceptionistKpisAsync();
            return Json(kpis);
        }

        [HttpGet]
        public async Task<IActionResult> ActiveSessions()
        {
            var sessions = await _dashboardService.GetActiveSessionsAsync(DateTime.UtcNow);
            return Json(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> TodaySchedule()
        {
            var schedule = await _dashboardService.GetTodayScheduleAsync(DateTime.UtcNow.Date);
            return Json(schedule);
        }
    }
}
