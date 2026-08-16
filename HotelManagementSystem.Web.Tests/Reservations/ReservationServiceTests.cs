using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using Xunit;

namespace HotelManagementSystem.Web.Tests;

public sealed class ReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreatePendingReservation_WhenRoomIsAvailable()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(capacity: 3);
        var command = new CreateReservationCommand(guest.Id, room.Id, new DateOnly(2030, 2, 1), new DateOnly(2030, 2, 4), 2);

        var result = await fixture.Reservations.CreateAsync(command);

        Assert.Equal(ReservationStatus.Pending, result.Status);
        Assert.Equal(guest.Id, result.GuestId);
        Assert.Equal(room.RoomNumber, result.RoomNumber);
        Assert.Equal(command.CheckIn, result.CheckIn);
        Assert.Equal(command.CheckOut, result.CheckOut);
        Assert.Equal(command.GuestsCount, result.GuestsCount);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenCheckOutIsBeforeCheckIn()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var initialCount = fixture.DbContext.Reservations.Count();

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>
            (() => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, 
            new DateOnly(2030, 2, 4), new DateOnly(2030, 2, 1), 1)));

        Assert.Equal(PersistenceErrorCode.InvalidReservationPeriod, exception.ErrorCode);
        Assert.Equal(initialCount, fixture.DbContext.Reservations.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenCheckOutEqualsCheckIn()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var date = new DateOnly(2030, 2, 4);

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>(() 
            => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, date, date, 1)));

        Assert.Equal(PersistenceErrorCode.InvalidReservationPeriod, exception.ErrorCode);
        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenGuestsCountIsZero()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() 
            => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, 
            new DateOnly(2030, 2, 1), new DateOnly(2030, 2, 2), 0)));

        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenGuestsCountExceedsRoomCapacity()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(capacity: 2);

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>
            (() => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, 
            new DateOnly(2030, 2, 1), new DateOnly(2030, 2, 2), 3)));

        Assert.Equal(PersistenceErrorCode.RoomCapacityExceeded, exception.ErrorCode);
        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenRoomIsNotClean()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(status: RoomOperationalStatus.Cleaning);

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>(() 
            => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, new DateOnly(2030, 2, 1), 
            new DateOnly(2030, 2, 2), 1)));

        Assert.Equal(PersistenceErrorCode.RoomUnavailable, exception.ErrorCode);
        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenRoomIdDoesNotExist()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>(() 
            => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, 9999, new DateOnly(2030, 2, 1), 
            new DateOnly(2030, 2, 2), 1)));

        Assert.Equal(PersistenceErrorCode.RoomUnavailable, exception.ErrorCode);
        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Theory]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.CheckedIn)]
    public async Task CreateAsync_ShouldReject_WhenDatesOverlapActiveReservation
        (ReservationStatus activeStatus)
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        fixture.AddReservation(guest, room, new DateOnly(2030, 2, 10), 
            new DateOnly(2030, 2, 15), status: activeStatus);
        var initialCount = fixture.DbContext.Reservations.Count();

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>
            (() => fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, new DateOnly(2030, 2, 12), 
            new DateOnly(2030, 2, 18), 1)));

        Assert.Equal(PersistenceErrorCode.RoomAlreadyReserved, exception.ErrorCode);
        Assert.Equal(initialCount, fixture.DbContext.Reservations.Count());
    }

    [Theory]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.CheckedOut)]
    public async Task CreateAsync_ShouldAllowDatesAfterNonActiveReservation
        (ReservationStatus inactiveStatus)
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        fixture.AddReservation(guest, room, new DateOnly(2030, 2, 10), 
            new DateOnly(2030, 2, 15), status: inactiveStatus);

        var result = await fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, 
            new DateOnly(2030, 2, 12), new DateOnly(2030, 2, 18), 1));

        Assert.Equal(ReservationStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldApplyPaginationStatusGuestAndRoomFilters()
    {
        await using var fixture = new ReservationTestFixture();
        var alice = fixture.AddGuest("Alice", "Smith", "alice@example.com");
        var bob = fixture.AddGuest("Bob", "Brown", "bob@example.com");
        var room101 = fixture.AddRoom("101");
        var room202 = fixture.AddRoom("202");
        fixture.AddReservation(alice, room101, new DateOnly(2030, 3, 1), 
            new DateOnly(2030, 3, 3), status: ReservationStatus.Confirmed);
        fixture.AddReservation(bob, room202, new DateOnly(2030, 3, 2), 
            new DateOnly(2030, 3, 4), status: ReservationStatus.Cancelled);
        fixture.AddReservation(alice, room202, new DateOnly(2030, 3, 3), 
            new DateOnly(2030, 3, 5), status: ReservationStatus.Confirmed);

        var page = await fixture.Reservations.GetPagedAsync(
            status: ReservationStatus.Confirmed,
            guestSearch: "alice",
            roomNumber: "202",
            pageNumber: 1,
            pageSize: 1);

        Assert.Equal(1, page.TotalCount);
        Assert.Single(page.Items);
        Assert.Equal("202", page.Items[0].RoomNumber);
        Assert.Equal("Alice Smith", page.Items[0].GuestName);
        Assert.Equal(1, page.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnSecondPage()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        fixture.AddReservation(guest, room, new DateOnly(2030, 4, 1), 
            new DateOnly(2030, 4, 2));
        var secondGuest = fixture.AddGuest("Second", "Guest", "second@example.com");
        var secondRoom = fixture.AddRoom("102");
        fixture.AddReservation(secondGuest, secondRoom, new DateOnly(2030, 4, 3), 
            new DateOnly(2030, 4, 4));

        var page = await fixture.Reservations.GetPagedAsync(pageNumber: 2, pageSize: 1);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.PageNumber);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDatesGuestAndGuestsCount()
    {
        await using var fixture = new ReservationTestFixture();
        var originalGuest = fixture.AddGuest();
        var replacementGuest = fixture.AddGuest("Jane", "Doe", "jane@example.com");
        var room = fixture.AddRoom(capacity: 3);
        var reservation = fixture.AddReservation(originalGuest, room, guestsCount: 1);

        var updated = await fixture.Reservations.UpdateAsync
            (new UpdateReservationCommand(
            reservation.Id, replacementGuest.Id, new DateOnly(2030, 5, 1), 
            new DateOnly(2030, 5, 5), 3));
        var saved = await fixture.DbContext.Reservations.FindAsync(reservation.Id);

        Assert.True(updated);
        Assert.NotNull(saved);
        Assert.Equal(replacementGuest.Id, saved.GuestId);
        Assert.Equal(new DateOnly(2030, 5, 1), saved.CheckIn);
        Assert.Equal(new DateOnly(2030, 5, 5), saved.CheckOut);
        Assert.Equal(3, saved.GuestsCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectCancelledReservationWithoutChangingIt()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var reservation = fixture.AddReservation(guest, room, status: 
            ReservationStatus.Cancelled);

        var updated = await fixture.Reservations.UpdateAsync
            (new UpdateReservationCommand(
            reservation.Id, guest.Id, new DateOnly(2030, 6, 1), 
            new DateOnly(2030, 6, 2), 2));
        var saved = await fixture.DbContext.Reservations.FindAsync(reservation.Id);

        Assert.False(updated);
        Assert.Equal(new DateOnly(2030, 1, 10), saved!.CheckIn);
        Assert.Equal(ReservationStatus.Cancelled, saved.Status);
        Assert.Equal(1, saved.GuestsCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectWhenNewDatesOverlapAnotherReservation()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var reservation = fixture.AddReservation(guest, room, 
            new DateOnly(2030, 7, 1), new DateOnly(2030, 7, 3));
        fixture.AddReservation(guest, room, new DateOnly(2030, 7, 10), 
            new DateOnly(2030, 7, 15), status: ReservationStatus.Confirmed);

        await Assert.ThrowsAsync<PersistenceOperationException>(() => 
        fixture.Reservations.UpdateAsync(new UpdateReservationCommand(
            reservation.Id, guest.Id, new DateOnly(2030, 7, 12), 
            new DateOnly(2030, 7, 14), 1)));

        var saved = await fixture.DbContext.Reservations.FindAsync(reservation.Id);
        Assert.Equal(new DateOnly(2030, 7, 1), saved!.CheckIn);
        Assert.Equal(new DateOnly(2030, 7, 3), saved.CheckOut);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalseForUnknownReservationOrGuest()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var reservation = fixture.AddReservation(guest, room);

        var unknownReservation = await fixture.Reservations.UpdateAsync
            (new UpdateReservationCommand(9999, guest.Id, new DateOnly(2030, 8, 1), 
            new DateOnly(2030, 8, 2), 1));
        var unknownGuest = await fixture.Reservations.UpdateAsync
            (new UpdateReservationCommand(reservation.Id, 9999, new DateOnly(2030, 8, 1),
            new DateOnly(2030, 8, 2), 1));

        Assert.False(unknownReservation);
        Assert.False(unknownGuest);
        Assert.Equal(new DateOnly(2030, 1, 10), (await fixture.DbContext.Reservations.
            FindAsync(reservation.Id))!.CheckIn);
    }

    [Fact]
    public async Task CheckInAsync_ShouldChangeConfirmedReservationToCheckedIn()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(status: RoomOperationalStatus.Clean);
        var reservation = fixture.AddReservation(guest, room, status: ReservationStatus.
            Confirmed);

        var changed = await fixture.Reservations.CheckInAsync(reservation.Id);

        Assert.True(changed);
        Assert.Equal(ReservationStatus.CheckedIn, (await fixture.DbContext.Reservations.
            FindAsync(reservation.Id))!.Status);
    }

    [Fact]
    public async Task CheckInAsync_ShouldRejectWhenRoomIsNotClean()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(status: RoomOperationalStatus.Cleaning);
        var reservation = fixture.AddReservation(guest, room, status: ReservationStatus.
            Confirmed);

        var changed = await fixture.Reservations.CheckInAsync(reservation.Id);

        Assert.False(changed);
        Assert.Equal(ReservationStatus.Confirmed, (await fixture.DbContext.Reservations.
            FindAsync(reservation.Id))!.Status);
    }

    [Theory]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.CheckedIn)]
    [InlineData(ReservationStatus.CheckedOut)]
    public async Task CheckInAsync_ShouldRejectForbiddenStatuses(ReservationStatus 
        status)
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var reservation = fixture.AddReservation(guest, room, status: status);

        var changed = await fixture.Reservations.CheckInAsync(reservation.Id);

        Assert.False(changed);
        Assert.Equal(status, (await fixture.DbContext.Reservations.FindAsync
            (reservation.Id))!.Status);
    }

    [Fact]
    public async Task CheckOutAsync_ShouldChangeCheckedInReservationToCheckedOutAndRoomToCleaning()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(status: RoomOperationalStatus.Clean);
        var reservation = fixture.AddReservation(guest, room, status: ReservationStatus.
            CheckedIn);

        var changed = await fixture.Reservations.CheckOutAsync(reservation.Id);
        await fixture.DbContext.Entry(room).ReloadAsync();

        Assert.True(changed);
        Assert.Equal(ReservationStatus.CheckedOut, (await fixture.DbContext.Reservations.
            FindAsync(reservation.Id))!.Status);
        Assert.Equal(RoomOperationalStatus.Cleaning, room.OperationalStatus);
    }

    [Fact]
    public async Task CheckOutAsync_ShouldRejectConfirmedReservation()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var reservation = fixture.AddReservation(guest, room, status: ReservationStatus.
            Confirmed);

        var changed = await fixture.Reservations.CheckOutAsync(reservation.Id);

        Assert.False(changed);
        Assert.Equal(ReservationStatus.Confirmed, (await fixture.DbContext.Reservations.
            FindAsync(reservation.Id))!.Status);
    }

    [Fact]
    public async Task CancelAsync_ShouldSetCancelledAndRoomClean()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(status: RoomOperationalStatus.Cleaning);
        var reservation = fixture.AddReservation(guest, room, status: ReservationStatus.
            Confirmed);

        var changed = await fixture.Reservations.CancelAsync(reservation.Id);
        await fixture.DbContext.Entry(room).ReloadAsync();

        Assert.True(changed);
        Assert.Equal(ReservationStatus.Cancelled, (await fixture.DbContext.Reservations.
            FindAsync(reservation.Id))!.Status);
        Assert.Equal(RoomOperationalStatus.Clean, room.OperationalStatus);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExistingReservation()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        var reservation = fixture.AddReservation(guest, room);

        var deleted = await fixture.Reservations.DeleteAsync(reservation.Id);

        Assert.True(deleted);
        Assert.Null(await fixture.DbContext.Reservations.FindAsync(reservation.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalseForUnknownReservation()
    {
        await using var fixture = new ReservationTestFixture();

        var deleted = await fixture.Reservations.DeleteAsync(9999);

        Assert.False(deleted);
        Assert.Empty(fixture.DbContext.Reservations);
    }
}
