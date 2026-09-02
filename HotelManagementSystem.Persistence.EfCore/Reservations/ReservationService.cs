using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Reservations;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Domain.Reservations;
using HotelManagementSystem.Domain.Rooms;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Persistence.EfCore.Reservations;

public sealed class ReservationService(ApplicationDbContext dbContext)
    : IReservationService
{
    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .OrderByDescending(reservation => reservation.CheckIn)
            .Select(reservation => new ReservationDto(
                reservation.Id,
                reservation.GuestId,
                reservation.Guest.FirstName + " " + reservation.Guest.LastName,
                reservation.Guest.Email,
                reservation.Guest.Phone,
                reservation.Room.RoomNumber,
                reservation.Room.Floor,
                reservation.Room.Type,
                reservation.Room.PricePerDay,
                reservation.Room.Capacity,
                reservation.CheckIn,
                reservation.CheckOut,
                reservation.GuestsCount,
                reservation.Status))
            .ToListAsync(cancellationToken);

        return reservations;
    }

    public async Task<PagedResult<ReservationDto>> GetPagedAsync(
        ReservationStatus? status = null,
        DateOnly? checkInFrom = null,
        DateOnly? checkInTo = null,
        DateOnly? checkOutFrom = null,
        DateOnly? checkOutTo = null,
        string? guestSearch = null,
        string? roomNumber = null,
        int pageNumber = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = pageSize <= 0 ? 30 : pageSize;

        var query = ApplyReservationFilters(
            dbContext.Reservations
                .AsNoTracking(),
            status,
            checkInFrom,
            checkInTo,
            checkOutFrom,
            checkOutTo,
            guestSearch,
            roomNumber);

        var totalCount = await query.CountAsync(cancellationToken);

        var reservations = await query
            .OrderByDescending(reservation => reservation.CheckIn)
            .ThenBy(reservation => reservation.Room.RoomNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(reservation => new ReservationDto(
                reservation.Id,
                reservation.GuestId,
                reservation.Guest.FirstName + " " + reservation.Guest.LastName,
                reservation.Guest.Email,
                reservation.Guest.Phone,
                reservation.Room.RoomNumber,
                reservation.Room.Floor,
                reservation.Room.Type,
                reservation.Room.PricePerDay,
                reservation.Room.Capacity,
                reservation.CheckIn,
                reservation.CheckOut,
                reservation.GuestsCount,
                reservation.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<ReservationDto>(
            reservations,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<ReservationDto> CreateAsync(
        CreateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        await EnsureAvailableAsync(
            command.RoomId,
            command.CheckIn,
            command.CheckOut,
            command.GuestsCount,
            null,
            cancellationToken);

        var reservation = new Reservation(
            command.GuestId,
            command.RoomId,
            command.CheckIn,
            command.CheckOut,
            command.GuestsCount);

        dbContext.Reservations.Add(reservation);

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(reservation)
            .Reference(item => item.Guest)
            .LoadAsync(cancellationToken);

        await dbContext.Entry(reservation)
            .Reference(item => item.Room)
            .LoadAsync(cancellationToken);

        return ToDto(reservation);
    }

    public async Task<bool> UpdateAsync(
        UpdateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .SingleOrDefaultAsync(
                item => item.Id == command.Id,
                cancellationToken);

        if (reservation is null ||
            reservation.Status == ReservationStatus.Cancelled)
        {
            return false;
        }

        var guestExists = await dbContext.Guests
            .AnyAsync(
                guest => guest.Id == command.GuestId,
                cancellationToken);

        if (!guestExists)
        {
            return false;
        }

        await EnsureAvailableAsync(
            reservation.RoomId,
            command.CheckIn,
            command.CheckOut,
            command.GuestsCount,
            command.Id,
            cancellationToken);

        reservation.Update(
            command.GuestId,
            command.CheckIn,
            command.CheckOut,
            command.GuestsCount);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ChangeStatusAsync(
        ChangeReservationStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .Include(item => item.Room)
            .SingleOrDefaultAsync(
                item => item.Id == command.Id,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        reservation.ChangeStatus(command.Status);

        if (command.Status == ReservationStatus.CheckedOut)
        {
            reservation.Room.ChangeOperationalStatus(
                RoomOperationalStatus.Cleaning);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CheckInAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .Include(item => item.Room)
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (reservation is null ||
            reservation.Status is
                ReservationStatus.Cancelled or
                ReservationStatus.CheckedIn or
                ReservationStatus.CheckedOut ||
            reservation.Room.OperationalStatus != RoomOperationalStatus.Clean)
        {
            return false;
        }

        reservation.ChangeStatus(ReservationStatus.CheckedIn);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CheckOutAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .Include(item => item.Room)
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (reservation is null ||
            reservation.Status != ReservationStatus.CheckedIn)
        {
            return false;
        }

        reservation.ChangeStatus(ReservationStatus.CheckedOut);
        reservation.Room.ChangeOperationalStatus(
            RoomOperationalStatus.Cleaning);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .Include(item => item.Room)
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        reservation.ChangeStatus(ReservationStatus.Cancelled);
        reservation.Room.ChangeOperationalStatus(
            RoomOperationalStatus.Clean);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.Reservations
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        dbContext.Reservations.Remove(reservation);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task EnsureAvailableAsync(
        int roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestsCount,
        int? ignoredId,
        CancellationToken cancellationToken)
    {
        ValidateReservationPeriod(checkIn, checkOut);

        var room = await dbContext.Rooms
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == roomId,
                cancellationToken);

        if (room is null ||
            room.OperationalStatus != RoomOperationalStatus.Clean)
        {
            throw new PersistenceOperationException(
                PersistenceErrorCode.RoomUnavailable);
        }

        if (guestsCount > room.Capacity)
        {
            throw new PersistenceOperationException(
                PersistenceErrorCode.RoomCapacityExceeded,
                room.Capacity);
        }

        var overlaps = await dbContext.Reservations
            .AnyAsync(
                reservation =>
                    reservation.RoomId == roomId &&
                    (reservation.Status == ReservationStatus.Pending ||
                     reservation.Status == ReservationStatus.Confirmed ||
                     reservation.Status == ReservationStatus.CheckedIn) &&
                    (!ignoredId.HasValue ||
                     reservation.Id != ignoredId.Value) &&
                    reservation.CheckIn < checkOut &&
                    checkIn < reservation.CheckOut,
                cancellationToken);

        if (overlaps)
        {
            throw new PersistenceOperationException(
                PersistenceErrorCode.RoomAlreadyReserved);
        }
    }

    private static void ValidateReservationPeriod(
        DateOnly checkIn,
        DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new PersistenceOperationException(
                PersistenceErrorCode.InvalidReservationPeriod);
        }
    }

    private static IQueryable<Reservation> ApplyReservationFilters(
        IQueryable<Reservation> query,
        ReservationStatus? status,
        DateOnly? checkInFrom,
        DateOnly? checkInTo,
        DateOnly? checkOutFrom,
        DateOnly? checkOutTo,
        string? guestSearch,
        string? roomNumber)
    {
        if (status.HasValue)
        {
            query = query.Where(
                reservation => reservation.Status == status.Value);
        }

        if (checkInFrom.HasValue)
        {
            query = query.Where(
                reservation => reservation.CheckIn >= checkInFrom.Value);
        }

        if (checkInTo.HasValue)
        {
            query = query.Where(
                reservation => reservation.CheckIn <= checkInTo.Value);
        }

        if (checkOutFrom.HasValue)
        {
            query = query.Where(
                reservation => reservation.CheckOut >= checkOutFrom.Value);
        }

        if (checkOutTo.HasValue)
        {
            query = query.Where(
                reservation => reservation.CheckOut <= checkOutTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(guestSearch))
        {
            var normalizedGuestSearch = guestSearch.Trim().ToLower();

            query = query.Where(
                reservation =>
                    reservation.Guest.FirstName
                        .ToLower()
                        .Contains(normalizedGuestSearch) ||
                    reservation.Guest.LastName
                        .ToLower()
                        .Contains(normalizedGuestSearch) ||
                    (reservation.Guest.FirstName + " " +
                     reservation.Guest.LastName)
                        .ToLower()
                        .Contains(normalizedGuestSearch));
        }

        if (!string.IsNullOrWhiteSpace(roomNumber))
        {
            var normalizedRoomNumber = roomNumber.Trim().ToLower();

            query = query.Where(
                reservation =>
                    reservation.Room.RoomNumber
                        .ToLower()
                        .Contains(normalizedRoomNumber));
        }

        return query;
    }

    private static ReservationDto ToDto(Reservation reservation)
    {
        return new ReservationDto(
            reservation.Id,
            reservation.GuestId,
            reservation.Guest.FirstName + " " + reservation.Guest.LastName,
            reservation.Guest.Email,
            reservation.Guest.Phone,
            reservation.Room.RoomNumber,
            reservation.Room.Floor,
            reservation.Room.Type,
            reservation.Room.PricePerDay,
            reservation.Room.Capacity,
            reservation.CheckIn,
            reservation.CheckOut,
            reservation.GuestsCount,
            reservation.Status);
    }
}