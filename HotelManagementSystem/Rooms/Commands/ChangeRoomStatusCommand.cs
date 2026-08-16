using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Application.Rooms.Commands;

public sealed record ChangeRoomStatusCommand(
    int Id,
    RoomOperationalStatus OperationalStatus) : ICommand<Result<Unit>>;
