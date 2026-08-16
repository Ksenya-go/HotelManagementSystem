using HotelManagementSystem.Domain.Reservation;

namespace HotelManagementSystem.Application.Reservations;

public sealed record ReservationDto(int Id, int GuestId, string GuestName, string GuestEmail, 
    string GuestPhone, string RoomNumber, int RoomFloor, string RoomType, decimal RoomPricePerDay, 
    int RoomCapacity, DateOnly CheckIn, DateOnly CheckOut, int GuestsCount, ReservationStatus Status);

