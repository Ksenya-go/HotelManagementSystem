using HotelManagementSystem.Application.Dashboard;

namespace HotelManagementSystem.Application.Services;

public interface IManagerReportingService
{
    Task<ManagerSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<ManagerReportDto> GetReportAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

