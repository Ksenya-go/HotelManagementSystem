using System.ComponentModel.DataAnnotations;
using HotelManagementSystem.Application.Reservations;
using HotelManagementSystem.Domain.Reservation;

namespace HotelManagementSystem.Web.ViewModels.Reservation;

public sealed class ReservationsIndexViewModel
{
    public IReadOnlyList<ReservationDto> Reservations { get; set; } = [];
    public ReservationStatus? Status { get; set; }
    public string CheckInTime { get; set; } = "14:00";
    public string CheckOutTime { get; set; } = "12:00";

    [DataType(DataType.Date)]
    public DateOnly? CheckInFrom { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? CheckInTo { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? CheckOutFrom { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? CheckOutTo { get; set; }

    public string? GuestSearch { get; set; }
    public string? RoomNumber { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

