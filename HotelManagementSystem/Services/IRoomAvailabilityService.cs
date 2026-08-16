using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Application.Services;

public interface IRoomAvailabilityService
{
    Task<RoomAvailabilityStatus> GetStatusAsync(int roomId, DateOnly date, CancellationToken cancellationToken = default);
    Task<RoomAvailabilityStatus> GetStatusForRangeAsync(int roomId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

