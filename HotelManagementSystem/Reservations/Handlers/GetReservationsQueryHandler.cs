using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.Reservations.Queries;
using HotelManagementSystem.Application.Services;

namespace HotelManagementSystem.Application.Reservations.Handlers;

public sealed class GetReservationsQueryHandler(
    IReservationService reservationService)
    : IQueryHandler<GetReservationsQuery, Result<PagedResult<ReservationDto>>>
{
    public async ValueTask<Result<PagedResult<ReservationDto>>> Handle(
        GetReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var reservations = await reservationService.GetPagedAsync(
            request.Status,
            request.CheckInFrom,
            request.CheckInTo,
            request.CheckOutFrom,
            request.CheckOutTo,
            request.GuestSearch,
            request.RoomNumber,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PagedResult<ReservationDto>>.Ok(reservations);
    }
}
