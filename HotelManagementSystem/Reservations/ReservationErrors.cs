using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Reservations;

public static class ReservationErrors
{
    public static Result<ReservationDto> Invalid(string message) =>
        Result<ReservationDto>.Fail(
            "Reservation.Invalid",
            message);

    public static Result<Unit> InvalidUnit(string message) =>
        Result<Unit>.Fail(
            "Reservation.Invalid",
            message);

    public static Result<Unit> NotFound() =>
        Result<Unit>.Fail(
            "Reservation.NotFound",
            "Бронювання не знайдено.");

    public static Result<Unit> CheckInFailed() =>
        Result<Unit>.Fail(
            "Reservation.CheckInFailed",
            "Не вдалося заселити гостя.");

    public static Result<Unit> CheckOutFailed() =>
        Result<Unit>.Fail(
            "Reservation.CheckOutFailed",
            "Не вдалося виселити гостя.");
}
