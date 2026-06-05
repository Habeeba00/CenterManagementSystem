using CenterManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    /// <summary>
    /// Phase 6 — Notification endpoints for bell dropdown.
    /// </summary>
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetUnread()
            => Json(new { count = await _notificationService.GetUnreadCountAsync(CurrentUserId) });

        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
            => Json(await _notificationService.GetNotificationsAsync(CurrentUserId, page, 10));

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notificationService.MarkReadAsync(id, CurrentUserId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(CurrentUserId);
            return Json(new { success = true });
        }
    }
}
