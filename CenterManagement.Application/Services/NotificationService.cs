using CenterManagement.Application.DTOs.Notification;
using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;

namespace CenterManagement.Application.Services
{
    /// <summary>
    /// Phase 1 stub implementation. Phase 6 replaces this with the full implementation.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly CenterManagementDbContext _context;

        public NotificationService(CenterManagementDbContext context)
        {
            _context = context;
        }

        public async Task SendToUserAsync(string userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public Task SendToGroupAsync(int groupId, string title, string message)
        {
            Console.WriteLine($"[NotificationStub] SendToGroup({groupId}, \"{title}\", \"{message}\")");
            return Task.CompletedTask;
        }

        public Task<int> GetUnreadCountAsync(string userId)
        {
            return Task.FromResult(0);
        }

        public Task<List<NotificationDto>> GetNotificationsAsync(string userId, int page, int pageSize)
        {
            return Task.FromResult(new List<NotificationDto>());
        }

        public Task MarkReadAsync(int notificationId, string userId)
        {
            return Task.CompletedTask;
        }

        public Task MarkAllReadAsync(string userId)
        {
            return Task.CompletedTask;
        }
    }
}
