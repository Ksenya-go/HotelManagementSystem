using HotelManagementSystem.Application.SystemSettings;
using HotelManagementSystem.Application.SystemSettings.Commands;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Persistence.EfCore.SystemSettings;

public sealed class SystemSettingService(ApplicationDbContext dbContext)
    : ISystemSettingService
{
    public async Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.SystemSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Key)
            .Select(setting => new SystemSettingDto(
                setting.Id,
                setting.Key,
                setting.Value,
                setting.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        UpdateSystemSettingCommand command,
        CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.SystemSettings
            .SingleOrDefaultAsync(
                item => item.Id == command.Id,
                cancellationToken);

        if (setting is null)
        {
            return false;
        }

        setting.UpdateValue(command.Value);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}