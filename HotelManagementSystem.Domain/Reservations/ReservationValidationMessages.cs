namespace HotelManagementSystem.Domain.Reservations;

internal static class ReservationValidationMessages
{
    public const string CheckOutBeforeCheckIn = "Дата виселення має бути пізнішою за дату заселення.";
    public const string GuestsCountTooLow = "Кількість гостей має бути щонайменше 1.";
}
