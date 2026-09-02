using HotelManagementSystem.Application.Dashboard;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Domain.Reservations;
using HotelManagementSystem.Domain.Rooms;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Persistence.EfCore.Dashboard;

public sealed class ManagerReportingService(ApplicationDbContext dbContext)
    : IManagerReportingService
{
    public async Task<ManagerSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rooms = await dbContext.Rooms
            .AsNoTracking()
            .Select(room => new
            {
                room.Id,
                room.OperationalStatus
            })
            .ToListAsync(cancellationToken);

        var occupiedRoomIds = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                (reservation.Status == ReservationStatus.Confirmed ||
                 reservation.Status == ReservationStatus.CheckedIn) &&
                reservation.CheckIn <= today &&
                today < reservation.CheckOut)
            .Select(reservation => reservation.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var reservationsToday = await dbContext.Reservations
            .AsNoTracking()
            .CountAsync(
                reservation =>
                    reservation.CheckIn == today &&
                    reservation.Status != ReservationStatus.Cancelled,
                cancellationToken);

        var pendingReservations = await dbContext.Reservations
            .AsNoTracking()
            .CountAsync(
                reservation => reservation.Status == ReservationStatus.Pending,
                cancellationToken);

        var totalRooms = rooms.Count;

        var availableRooms = rooms.Count(room =>
            room.OperationalStatus == RoomOperationalStatus.Clean &&
            !occupiedRoomIds.Contains(room.Id));

        var occupiedRooms = occupiedRoomIds.Count;

        var cleaningRooms = rooms.Count(
            room => room.OperationalStatus == RoomOperationalStatus.Cleaning);

        var maintenanceRooms = rooms.Count(
            room => room.OperationalStatus == RoomOperationalStatus.InMaintenance);

        var occupancyRate = totalRooms == 0
            ? 0
            : Math.Round(
                (decimal)occupiedRooms / totalRooms * 100,
                1);

        return new ManagerSummaryDto(
            totalRooms,
            availableRooms,
            occupiedRooms,
            cleaningRooms,
            maintenanceRooms,
            reservationsToday,
            pendingReservations,
            occupancyRate);
    }

    public async Task<ManagerReportDto> GetReportAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation =>
                reservation.CheckIn < to &&
                reservation.CheckOut > from)
            .ToListAsync(cancellationToken);

        var totalRoomCount = await dbContext.Rooms
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var numberOfDays = Math.Max(1, to.DayNumber - from.DayNumber);
        var totalRoomDays = totalRoomCount * numberOfDays;

        var activeReservations = reservations.Count(
            reservation =>
                reservation.Status is
                    ReservationStatus.Pending or
                    ReservationStatus.Confirmed or
                    ReservationStatus.CheckedIn);

        var completedReservations = reservations.Count(
            reservation => reservation.Status == ReservationStatus.CheckedOut);

        var cancelledReservations = reservations.Count(
            reservation => reservation.Status == ReservationStatus.Cancelled);

        var nonCancelledReservations = reservations
            .Where(reservation =>
                reservation.Status != ReservationStatus.Cancelled)
            .ToList();

        var occupiedDays = nonCancelledReservations.Sum(
            reservation =>
                Math.Max(
                    0,
                    reservation.CheckOut.DayNumber -
                    reservation.CheckIn.DayNumber));

        var totalGuests = nonCancelledReservations.Sum(
            reservation => reservation.GuestsCount);

        var occupancyRate = totalRoomDays == 0
            ? 0
            : Math.Round(
                (decimal)occupiedDays / totalRoomDays * 100,
                1);

        var cancellationRate = reservations.Count == 0
            ? 0
            : Math.Round(
                (decimal)cancelledReservations / reservations.Count * 100,
                1);

        var averageStayDays = nonCancelledReservations.Count == 0
            ? 0
            : Math.Round(
                (decimal)occupiedDays / nonCancelledReservations.Count,
                1);

        return new ManagerReportDto(
            from,
            to,
            reservations.Count,
            activeReservations,
            completedReservations,
            cancelledReservations,
            totalGuests,
            occupiedDays,
            totalRoomDays,
            occupancyRate,
            cancellationRate,
            averageStayDays);
    }
}