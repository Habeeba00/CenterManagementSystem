namespace CenterManagement.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            string userId,
            string action,
            string entityName,
            int entityId,
            string? oldValues,
            string? newValues);
    }
}
