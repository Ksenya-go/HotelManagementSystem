using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Guests;

public static class GuestErrors
{
    public static Result<Unit> NotFound() =>
        Result<Unit>.Fail(
            "Guest.NotFound",
            "Гостя не знайдено.");
}
