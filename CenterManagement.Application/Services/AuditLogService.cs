using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;

namespace CenterManagement.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly CenterManagementDbContext _context;

        public AuditLogService(CenterManagementDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string userId,
            string action,
            string entityName,
            int entityId,
            string? oldValues,
            string? newValues)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    ActionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLog Error] {ex.Message}");
            }
        }
    }
}
