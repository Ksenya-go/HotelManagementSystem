using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using Mediator;

namespace HotelManagementSystem.Application.Guests;

public static class GuestErrors
{
    public static Result<GuestDto> Invalid(string message) =>
        Result.Fail<GuestDto>(
            new AppError(
                "Guest.Invalid",
                message));

    public static Result<Unit> NotFound() =>
        Result.Fail<Unit>(
            new AppError(
                "Guest.NotFound",
                "Гостя не знайдено."));
}
