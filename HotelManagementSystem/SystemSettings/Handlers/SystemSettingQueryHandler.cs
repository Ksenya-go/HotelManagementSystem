using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.SystemSettings.Handlers;

public sealed class SystemSettingQueryHandler(ISystemSettingService service) :
    IQueryHandler<Queries.GetSystemSettingsQuery, Result<IReadOnlyList<SystemSettingDto>>>
{
    public async ValueTask<Result<IReadOnlyList<SystemSettingDto>>> Handle(
        Queries.GetSystemSettingsQuery request,
        CancellationToken cancellationToken) =>
        Result<IReadOnlyList<SystemSettingDto>>.Ok(await service.GetAllAsync(cancellationToken));
}
