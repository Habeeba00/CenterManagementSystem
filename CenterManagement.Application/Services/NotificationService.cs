using CenterManagement.Application.DTOs.Notification;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Application.Services
{
    /// <summary>
    /// Phase 6 full implementation — replaces the Phase 1 stub.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly CenterManagementDbContext _db;

        public NotificationService(CenterManagementDbContext db)
        {
            _db = db;
        }

        public async Task SendToUserAsync(string userId, string title, string message)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task SendToGroupAsync(int groupId, string title, string message)
        {
            var userIds = await _db.Enrollments
                .Where(e => e.GroupId == groupId && e.IsActive)
                .Select(e => e.StudentProfile.UserId)
                .Distinct()
                .ToListAsync();

            // Single batch — do NOT call SaveChangesAsync in a loop
            var notifications = userIds.Select(uid => new Notification
            {
                UserId = uid,
                Title = title,
                Message = message,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _db.Notifications.AddRangeAsync(notifications);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
            => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<List<NotificationDto>> GetNotificationsAsync(
            string userId, int page, int pageSize)
            => await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    SentAt = n.SentAt
                })
                .ToListAsync();

        public async Task MarkReadAsync(int notificationId, string userId)
        {
            var n = await _db.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
            if (n is null) return;
            n.IsRead = true;
            n.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(string userId)
        {
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedAt, DateTime.UtcNow));
        }
    }
}
