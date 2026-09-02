using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.RoomTypes;
using Mediator;

namespace HotelManagementSystem.Application.Rooms;

public static class RoomErrors
{
    public static Result<IReadOnlyList<RoomDto>> DuplicateRoomNumber() =>
        Result.Fail<IReadOnlyList<RoomDto>>(
            new AppError(
                "Room.DuplicateRoomNumber",
                "Room.DuplicateRoomNumber"));

    public static Result<Unit> NotFound() =>
       Result.Fail<Unit>(
            new AppError(
            "Room.NotFound",
            "Room.NotFound"));

    public static Result<Unit> DeleteFailed() =>
        Result.Fail<Unit>(
            new AppError(
            "Room.DeleteFailed",
            "Room.DeleteFailed"));
}
