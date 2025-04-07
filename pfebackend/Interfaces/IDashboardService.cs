using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GetDashboardDataAsync(string userId);
    }
}
