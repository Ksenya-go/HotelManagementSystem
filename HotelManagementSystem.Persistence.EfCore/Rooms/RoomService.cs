using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.Common.Presentation;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Persistence.EfCore.Rooms;

public sealed class RoomService(ApplicationDbContext dbContext)
    : IRoomService
{
    public async Task<IReadOnlyList<RoomDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var rooms = await dbContext.Rooms
            .AsNoTracking()
            .OrderBy(room => room.RoomNumber)
            .ToListAsync(cancellationToken);

        var occupiedRoomIds = await GetOccupiedRoomIdsAsync(
            today,
            today.AddDays(1),
            cancellationToken);

        return rooms
            .Select(room => ToDto(
                room,
                occupiedRoomIds.Contains(room.Id)))
            .ToList();
    }

    public async Task<PagedResult<RoomDto>> GetPagedAsync(
        int? floor = null,
        string? roomType = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 30 : pageSize;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = ApplyRoomFilters(
            dbContext.Rooms.AsNoTracking(),
            floor,
            roomType,
            minPrice,
            maxPrice);

        var totalCount = await query.CountAsync(cancellationToken);

        var rooms = await query
            .OrderBy(room => room.RoomNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var occupiedRoomIds = await GetOccupiedRoomIdsAsync(
            today,
            today.AddDays(1),
            cancellationToken);

        var items = rooms
            .Select(room => ToDto(
                room,
                occupiedRoomIds.Contains(room.Id)))
            .ToList();

        return new PagedResult<RoomDto>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<PagedResult<RoomPeriodStatusDto>> GetPeriodStatusesAsync(
        DateOnly startDate,
        DateOnly endDate,
        int? floor = null,
        string? roomType = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? guestsCount = null,
        int pageNumber = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        ValidatePeriod(startDate, endDate);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 30 : pageSize;

        var query = ApplyRoomFilters(
            dbContext.Rooms.AsNoTracking(),
            floor,
            roomType,
            minPrice,
            maxPrice);

        if (guestsCount.HasValue)
        {
            query = query.Where(
                room => room.Capacity >= guestsCount.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rooms = await query
            .OrderBy(room => room.RoomNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(room => new
            {
                room.Id,
                room.RoomNumber,
                room.Floor,
                room.Type,
                room.Description,
                room.PricePerDay,
                room.Capacity,
                room.RoomCount,
                room.OperationalStatus
            })
            .ToListAsync(cancellationToken);

        var occupiedRoomIds = await GetOccupiedRoomIdsAsync(
            startDate,
            endDate,
            cancellationToken);

        var items = rooms
            .Select(room =>
            {
                var isOccupied = occupiedRoomIds.Contains(room.Id);

                var operationalStatus = RoomStatusText.GetOperationalStatus(
                    room.OperationalStatus);

                var availabilityStatus = RoomStatusText.GetAvailabilityStatus(
                    isOccupied);

                var isAvailable =
                    room.OperationalStatus == RoomOperationalStatus.Clean &&
                    !isOccupied;

                return new RoomPeriodStatusDto(
                    room.Id,
                    room.RoomNumber,
                    room.Floor,
                    room.Type,
                    room.Description,
                    room.PricePerDay,
                    room.Capacity,
                    room.RoomCount,
                    operationalStatus,
                    availabilityStatus,
                    isAvailable);
            })
            .ToList();

        return new PagedResult<RoomPeriodStatusDto>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<IReadOnlyList<RoomDto>> CreateAsync(
        CreateRoomCommand command,
        CancellationToken cancellationToken = default)
    {
        var roomExists = await dbContext.Rooms
            .AnyAsync(
                room => room.RoomNumber == command.RoomNumber,
                cancellationToken);

        if (roomExists)
        {
            return [];
        }

        var room = new Room(
            command.RoomNumber,
            command.Floor,
            command.Type,
            command.Description,
            command.PricePerDay,
            command.Capacity,
            command.RoomCount,
            command.OperationalStatus);

        dbContext.Rooms.Add(room);

        await dbContext.SaveChangesAsync(cancellationToken);

        return [ToDto(room, false)];
    }

    public async Task<bool> ChangeOperationalStatusAsync(
        int id,
        RoomOperationalStatus operationalStatus,
        CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (room is null)
        {
            return false;
        }

        room.ChangeOperationalStatus(operationalStatus);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(
        UpdateRoomCommand command,
        CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms
            .SingleOrDefaultAsync(
                item => item.Id == command.Id,
                cancellationToken);

        if (room is null)
        {
            return false;
        }

        var duplicateExists = await dbContext.Rooms
            .AnyAsync(
                item =>
                    item.Id != command.Id &&
                    item.RoomNumber == command.RoomNumber,
                cancellationToken);

        if (duplicateExists)
        {
            return false;
        }

        room.Update(
            command.RoomNumber,
            command.Floor,
            command.Type,
            command.Description,
            command.PricePerDay,
            command.Capacity,
            command.RoomCount,
            command.OperationalStatus);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var room = await dbContext.Rooms
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (room is null)
        {
            return false;
        }

        var reservations = await dbContext.Reservations
            .Where(reservation => reservation.RoomId == id)
            .ToListAsync(cancellationToken);

        var hasActiveReservations = reservations.Any(
            reservation =>
                reservation.Status == ReservationStatus.Pending ||
                reservation.Status == ReservationStatus.Confirmed ||
                reservation.Status == ReservationStatus.CheckedIn);

        if (hasActiveReservations)
        {
            return false;
        }

        dbContext.Reservations.RemoveRange(reservations);
        dbContext.Rooms.Remove(room);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<HashSet<int>> GetOccupiedRoomIdsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var roomIds = await dbContext.Reservations
            .AsNoTracking()
            .Where(
                reservation =>
                    (reservation.Status == ReservationStatus.Confirmed ||
                     reservation.Status == ReservationStatus.CheckedIn) &&
                    reservation.CheckIn < to &&
                    from < reservation.CheckOut)
            .Select(reservation => reservation.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return roomIds.ToHashSet();
    }

    private static void ValidatePeriod(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
        {
            throw new PersistenceOperationException(
                PersistenceErrorCode.InvalidDateRange);
        }
    }

    private static IQueryable<Room> ApplyRoomFilters(
        IQueryable<Room> query,
        int? floor,
        string? roomType,
        decimal? minPrice,
        decimal? maxPrice)
    {
        if (floor.HasValue)
        {
            query = query.Where(
                room => room.Floor == floor.Value);
        }

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            query = query.Where(
                room => room.Type == roomType);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(
                room => room.PricePerDay >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(
                room => room.PricePerDay <= maxPrice.Value);
        }

        return query;
    }

    private static RoomDto ToDto(
        Room room,
        bool occupied)
    {
        return new RoomDto(
            room.Id,
            room.RoomNumber,
            room.Floor,
            room.Type,
            room.Description,
            room.PricePerDay,
            room.Capacity,
            room.RoomCount,
            room.BookedDates,
            room.OperationalStatus,
            occupied
                ? RoomAvailabilityStatus.Occupied
                : RoomAvailabilityStatus.Available);
    }
}