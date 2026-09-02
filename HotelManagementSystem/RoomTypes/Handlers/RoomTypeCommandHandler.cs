using FluentResults;
using HotelManagementSystem.Application.RoomTypes.Commands;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Handlers;

public sealed class RoomTypeCommandHandler(IRoomTypeService service)
    : ICommandHandler<CreateRoomTypeCommand, Result<RoomTypeDto>>,
      ICommandHandler<UpdateRoomTypeCommand, Result<Unit>>,
      ICommandHandler<DeleteRoomTypeCommand, Result<Unit>>
{
    public async ValueTask<Result<RoomTypeDto>> Handle(CreateRoomTypeCommand request,
        CancellationToken cancellationToken)
    {
        var roomType = await service.CreateAsync(request, cancellationToken);

        return Result.Ok(roomType);
    }

    public async ValueTask<Result<Unit>> Handle(UpdateRoomTypeCommand request,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(request, cancellationToken);

        return updated
            ? Result.Ok(Unit.Value)
            : RoomTypeErrors.NotFound();
    }

    public async ValueTask<Result<Unit>> Handle(DeleteRoomTypeCommand request,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(
            request.Id,
            cancellationToken);

        return deleted
            ? Result.Ok(Unit.Value)
            : RoomTypeErrors.DeleteFailed();
    }
}