using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CenterManagement.Web.Controllers
{
    /// <summary>
    /// Phase 6 — Analytics/Insights page (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Kpis()
        {
            var kpis = await _analyticsService.GetAnalyticsKpisAsync();
            return Json(kpis);
        }

        [HttpGet]
        public async Task<IActionResult> AttendanceTrend()
        {
            var trend = await _analyticsService.GetAttendanceTrend30DaysAsync();
            return Json(trend);
        }

        [HttpGet]
        public async Task<IActionResult> StudentDistribution()
        {
            var distribution = await _analyticsService.GetStudentDistributionByGradeAsync();
            return Json(distribution);
        }

        [HttpGet]
        public async Task<IActionResult> RevenueBySubject()
        {
            var revenue = await _analyticsService.GetRevenueBySubjectAsync();
            return Json(revenue);
        }

        [HttpGet]
        public async Task<IActionResult> TopTeachers()
        {
            var teachers = await _analyticsService.GetTopTeachersAsync(5);
            return Json(teachers);
        }

        [HttpGet]
        public async Task<IActionResult> SmartInsights()
        {
            var insights = await _analyticsService.GetSmartInsightsAsync();
            return Json(insights);
        }
    }
}
