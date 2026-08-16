namespace HotelManagementSystem.Application.Common.Errors;

public enum PersistenceErrorCode
{
    InvalidDateRange,
    InvalidReservationPeriod,
    RoomUnavailable,
    RoomCapacityExceeded,
    RoomAlreadyReserved
}

public sealed class PersistenceOperationException(
    PersistenceErrorCode errorCode,
    int? capacity = null)
    : Exception()
{
    public PersistenceErrorCode ErrorCode { get; } = errorCode;

    public int? Capacity { get; } = capacity;
}
