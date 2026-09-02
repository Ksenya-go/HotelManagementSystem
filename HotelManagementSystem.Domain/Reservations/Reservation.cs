using HotelManagementSystem.Domain.Guests;
using HotelManagementSystem.Domain.Rooms;

namespace HotelManagementSystem.Domain.Reservations;

public sealed class Reservation
{
    private Reservation()
    {
    }

    public Reservation(
        int guestId,
        int roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestsCount)
    {
        Validate(checkIn, checkOut, guestsCount);

        GuestId = guestId;
        RoomId = roomId;
        CheckIn = checkIn;
        CheckOut = checkOut;
        GuestsCount = guestsCount;
        Status = ReservationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public int GuestId { get; private set; }

    public Guest Guest { get; private set; } = null!;

    public int RoomId { get; private set; }

    public Room Room { get; private set; } = null!;

    public DateOnly CheckIn { get; private set; }

    public DateOnly CheckOut { get; private set; }

    public int GuestsCount { get; private set; }

    public ReservationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public void Update(
        int guestId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestsCount)
    {
        Validate(checkIn, checkOut, guestsCount);

        GuestId = guestId;
        CheckIn = checkIn;
        CheckOut = checkOut;
        GuestsCount = guestsCount;
    }

    public void ChangeStatus(ReservationStatus status)
    {
        Status = status;
    }

    private static void Validate(DateOnly checkIn,DateOnly checkOut,int guestsCount)
    {
        if (checkOut <= checkIn)
        {
            throw new ArgumentException(
                ReservationValidationMessages.CheckOutBeforeCheckIn);
        }

        if (guestsCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(guestsCount),
                ReservationValidationMessages.GuestsCountTooLow);
        }
    }
}