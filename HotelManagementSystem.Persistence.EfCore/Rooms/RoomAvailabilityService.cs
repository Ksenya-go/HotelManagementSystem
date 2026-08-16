using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Persistence.EfCore.Rooms;

public sealed class RoomAvailabilityService(ApplicationDbContext dbContext)
    : IRoomAvailabilityService
{
    public Task<RoomAvailabilityStatus> GetStatusAsync(
        int roomId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return GetStatusForRangeAsync(
            roomId,
            date,
            date.AddDays(1),
            cancellationToken);
    }

    public async Task<RoomAvailabilityStatus> GetStatusForRangeAsync(
        int roomId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(from, to);

        var occupied = await dbContext.Reservations
            .AsNoTracking()
            .AnyAsync(
                reservation =>
                    reservation.RoomId == roomId &&
                    (reservation.Status == ReservationStatus.Confirmed ||
                     reservation.Status == ReservationStatus.CheckedIn) &&
                    reservation.CheckIn < to &&
                    from < reservation.CheckOut,
                cancellationToken);

        return occupied
            ? RoomAvailabilityStatus.Occupied
            : RoomAvailabilityStatus.Available;
    }

    private static void ValidatePeriod(DateOnly from, DateOnly to)
    {
        if (to <= from)
        {
            throw new PersistenceOperationException(
                PersistenceErrorCode.InvalidDateRange);
        }
    }
}