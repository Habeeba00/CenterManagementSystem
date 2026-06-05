using CenterManagement.Application.DTOs.Dashboard;
using CenterManagement.Application.DTOs.Session;

namespace CenterManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardKpiDto> GetReceptionistKpisAsync();
        Task<List<ActiveSessionDto>> GetActiveSessionsAsync(DateTime now);
        Task<List<SessionScheduleDto>> GetTodayScheduleAsync(DateTime today);
    }
}
