using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.Reservations;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Domain.Reservation;

namespace HotelManagementSystem.Application.Services;

public interface IReservationService
{
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ReservationDto>> GetPagedAsync(ReservationStatus? status = null, DateOnly? checkInFrom = null, DateOnly? checkInTo = null, DateOnly? checkOutFrom = null, DateOnly? checkOutTo = null, string? guestSearch = null, string? roomNumber = null, int pageNumber = 1, int pageSize = 30, CancellationToken cancellationToken = default);
    Task<ReservationDto> CreateAsync(CreateReservationCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateReservationCommand command, CancellationToken cancellationToken = default);
    Task<bool> ChangeStatusAsync(ChangeReservationStatusCommand command, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CheckInAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CheckOutAsync(int id, CancellationToken cancellationToken = default);
}

