using CenterManagement.Application.DTOs.Common;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Web.Controllers
{
    /// <summary>
    /// Phase 6 — Audit Log viewer (Admin only).
    /// Read-only aggregation directly from DbContext — acceptable Clean Architecture exception.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly CenterManagementDbContext _db;

        public AuditLogController(CenterManagementDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? userId = null,
            string? entityName = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var query = _db.AuditLogs
                .Include(a => a.User)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(a => a.UserId == userId);
            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(a => a.EntityName.Contains(entityName));
            if (from.HasValue)
                query = query.Where(a => a.ActionDate >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.ActionDate <= to.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.ActionDate)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            ViewBag.UserId = userId;
            ViewBag.EntityName = entityName;
            ViewBag.From = from;
            ViewBag.To = to;

            return View(new PagedResult<AuditLog>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 20
            });
        }
    }
}
