using FluentResults;
using HotelManagementSystem.Domain.Room;
using Mediator;

namespace HotelManagementSystem.Application.Rooms.Commands;

public sealed record ChangeRoomStatusCommand(
    int Id,
    RoomOperationalStatus OperationalStatus) : ICommand<Result<Unit>>;
