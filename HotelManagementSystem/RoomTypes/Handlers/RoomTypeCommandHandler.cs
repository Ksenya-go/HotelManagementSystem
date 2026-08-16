using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.RoomTypes.Commands;


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

        return Result<RoomTypeDto>.Ok(roomType);
    }

    public async ValueTask<Result<Unit>> Handle(UpdateRoomTypeCommand request,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(request, cancellationToken);

        return updated
            ? Result<Unit>.Ok(Unit.Value)
            : RoomTypeErrors.NotFound();
    }

    public async ValueTask<Result<Unit>> Handle(DeleteRoomTypeCommand request,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(
            request.Id,
            cancellationToken);

        return deleted
            ? Result<Unit>.Ok(Unit.Value)
            : RoomTypeErrors.DeleteFailed();
    }
}