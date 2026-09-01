using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.RoomTypes;
using Mediator;

namespace HotelManagementSystem.Application.Rooms;

public static class RoomErrors
{
    public static Result<IReadOnlyList<RoomDto>> DuplicateRoomNumber() =>
        Result<IReadOnlyList<RoomDto>>.Fail(
            "Room.DuplicateRoomNumber",
            "Room.DuplicateRoomNumber");

    public static Result<Unit> NotFound() =>
        Result<Unit>.Fail(
            "Room.NotFound",
            "Room.NotFound");

    public static Result<Unit> DeleteFailed() =>
        Result<Unit>.Fail(
            "Room.DeleteFailed",
            "Room.DeleteFailed");
}
