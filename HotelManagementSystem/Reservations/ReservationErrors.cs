using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using Mediator;

namespace HotelManagementSystem.Application.Reservations;

public static class ReservationErrors
{
    public static Result<ReservationDto> Invalid(string message) =>
        Result.Fail<ReservationDto>(
            new AppError(
                "Reservation.Invalid",
                message));

    public static Result<Unit> InvalidUnit(string message) =>
        Result.Fail<Unit>(
            new AppError(
                "Reservation.Invalid",
                message));

    public static Result<Unit> NotFound() =>
        Result.Fail<Unit>(
            new AppError(
                "Reservation.NotFound",
                "Бронювання не знайдено."));

    public static Result<Unit> CheckInFailed() =>
        Result.Fail<Unit>(
            new AppError(
                "Reservation.CheckInFailed",
                "Не вдалося заселити гостя."));

    public static Result<Unit> CheckOutFailed() =>
        Result.Fail<Unit>(
            new AppError(
                "Reservation.CheckOutFailed",
                "Не вдалося виселити гостя."));
}
