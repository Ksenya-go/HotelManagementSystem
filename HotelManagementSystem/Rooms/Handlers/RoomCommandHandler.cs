
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Application.Services;
using Mediator;

namespace HotelManagementSystem.Application.Rooms.Handlers;

public sealed class RoomCommandHandler(IRoomService service)
    : ICommandHandler<CreateRoomCommand, Result<IReadOnlyList<RoomDto>>>,
      ICommandHandler<UpdateRoomCommand, Result<Unit>>,
      ICommandHandler<DeleteRoomCommand, Result<Unit>>,
      ICommandHandler<ChangeRoomStatusCommand, Result<Unit>>
{
    public async ValueTask<Result<IReadOnlyList<RoomDto>>> Handle(
        CreateRoomCommand request,
        CancellationToken cancellationToken)
    {
        var rooms = await service.CreateAsync(request, cancellationToken);

        return rooms.Count == 0
            ? RoomErrors.DuplicateRoomNumber()
            : Result<IReadOnlyList<RoomDto>>.Ok(rooms);
    }

    public async ValueTask<Result<Unit>> Handle(
        UpdateRoomCommand request,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(request, cancellationToken);

        return updated
            ? Result<Unit>.Ok(Unit.Value)
            : RoomErrors.NotFound();
    }

    public async ValueTask<Result<Unit>> Handle(
        DeleteRoomCommand request,
        CancellationToken cancellationToken)
    {
        var room = await service.DeleteAsync(
            request.Id,
            cancellationToken);

        return room
            ? Result<Unit>.Ok(Unit.Value)
            : RoomErrors.DeleteFailed();
    }

    public async ValueTask<Result<Unit>> Handle(
        ChangeRoomStatusCommand request,
        CancellationToken cancellationToken)
    {
        var updated = await service.ChangeOperationalStatusAsync(
            request.Id,
            request.OperationalStatus,
            cancellationToken);

        return updated
            ? Result<Unit>.Ok(Unit.Value)
            : RoomErrors.NotFound();
    }
}
