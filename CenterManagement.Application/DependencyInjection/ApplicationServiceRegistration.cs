using CenterManagement.Application.Interfaces;
using CenterManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CenterManagement.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<INotificationService, NotificationService>();
            // Each subsequent phase appends their own registrations here
            return services;
        }
    }
}
