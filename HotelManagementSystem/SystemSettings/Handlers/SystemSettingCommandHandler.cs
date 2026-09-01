using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.SystemSettings.Handlers;

public sealed class SystemSettingCommandHandler(ISystemSettingService service) :
    ICommandHandler<Commands.UpdateSystemSettingCommand, Result<Unit>>
{
    public async ValueTask<Result<Unit>> Handle(Commands.UpdateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(
            new Commands.UpdateSystemSettingCommand(request.Id, request.Value), cancellationToken);
        return updated
            ? Result.Ok(Unit.Value)
            : SystemSettingErrors.NotFound();
    }
}
