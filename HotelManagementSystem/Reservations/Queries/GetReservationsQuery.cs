using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Common.Pagination;

namespace HotelManagementSystem.Application.Reservations.Queries;

public sealed record GetReservationsQuery(ReservationStatus? Status = null, 
    DateOnly? CheckInFrom = null, DateOnly? CheckInTo = null, DateOnly? CheckOutFrom = null, 
    DateOnly? CheckOutTo = null, string? GuestSearch = null, string? RoomNumber = null, 
    int PageNumber = 1, int PageSize = 30) : IQuery<Result<PagedResult<ReservationDto>>>;

