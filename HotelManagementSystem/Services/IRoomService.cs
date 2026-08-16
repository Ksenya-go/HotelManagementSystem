using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Application.Services;

public interface IRoomService
{
    Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<RoomDto>> GetPagedAsync(int? floor = null, string? roomType = null, decimal? minPrice = null, decimal? maxPrice = null, int pageNumber = 1, int pageSize = 30, CancellationToken cancellationToken = default);
    Task<PagedResult<RoomPeriodStatusDto>> GetPeriodStatusesAsync(DateOnly startDate, DateOnly endDate, int? floor = null, string? roomType = null, decimal? minPrice = null, decimal? maxPrice = null, int? guestsCount = null, int pageNumber = 1, int pageSize = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoomDto>> CreateAsync(CreateRoomCommand command, CancellationToken cancellationToken = default);
    Task<bool> ChangeOperationalStatusAsync(int id, RoomOperationalStatus operationalStatus, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateRoomCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

