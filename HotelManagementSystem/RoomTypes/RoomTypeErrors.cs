using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes;

public static class RoomTypeErrors
{
    public static Result<Unit> NotFound() =>
        Result<Unit>.Fail(
            "RoomType.NotFound",
            "Тип номера не знайдено.");

    public static Result<Unit> DeleteFailed() =>
        Result<Unit>.Fail(
            "RoomType.DeleteFailed",
            "Тип номера не знайдено або він використовується номерами.");
}
