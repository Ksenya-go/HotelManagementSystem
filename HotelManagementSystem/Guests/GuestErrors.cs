using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.Guests;

public static class GuestErrors
{
    public static Result<Unit> NotFound() =>
        Result<Unit>.Fail(
            "Guest.NotFound",
            "Гостя не знайдено.");
}
