namespace HotelManagementSystem.Application.Common.Errors;

public static class PersistenceErrorMessages
{
    public static string GetMessage(PersistenceOperationException exception)
    {
        return exception.ErrorCode switch
        {
            PersistenceErrorCode.InvalidDateRange =>
                "Дата закінчення має бути пізнішою за дату початку.",
            PersistenceErrorCode.InvalidReservationPeriod =>
                "Дата виселення має бути пізнішою за дату заселення.",
            PersistenceErrorCode.RoomUnavailable => "Номер недоступний.",
            PersistenceErrorCode.RoomCapacityExceeded =>
                $"Номер розрахований максимум на {exception.Capacity} гостей.",
            PersistenceErrorCode.RoomAlreadyReserved =>
                "Номер уже заброньований на вибрані дати.",
            _ => "Не вдалося виконати операцію."
        };
    }
}
