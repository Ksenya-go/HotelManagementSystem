using HotelManagementSystem.Domain.Reservations;

namespace HotelManagementSystem.Web.ViewModels.Reservation;

public sealed class ReservationStatusViewModel
{
    public int Id { get; set; }
    public ReservationStatus Status { get; set; }
}

