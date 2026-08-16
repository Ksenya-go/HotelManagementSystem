namespace HotelManagementSystem.Application.SystemSettings;

using HotelManagementSystem.Application.SystemSettings.Commands;

public interface ISystemSettingService
{
    Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateSystemSettingCommand command, CancellationToken cancellationToken = default);
}

