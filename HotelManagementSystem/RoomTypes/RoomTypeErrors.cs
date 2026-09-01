using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes;

public static class RoomTypeErrors
{
    public static Result<Unit> NotFound() =>
        Result.Fail<Unit>(
            new AppError(
                "RoomType.NotFound",
                "Тип номера не знайдено."));

    public static Result<Unit> DeleteFailed() =>
        Result.Fail<Unit>(
            new AppError(
                "RoomType.DeleteFailed",
                "Тип номера не знайдено або він використовується номерами."));
}
