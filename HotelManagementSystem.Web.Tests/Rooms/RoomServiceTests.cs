using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagementSystem.Web.Tests.Rooms;

public sealed class RoomServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnRoomsAndPagination()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101");
        fixture.AddRoom("102");
        fixture.AddRoom("103");

        var page = await fixture.Rooms.GetPagedAsync(pageNumber: 2, pageSize: 2);

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.PageNumber);
        Assert.Equal(2, page.PageSize);
        Assert.Single(page.Items);
        Assert.Equal("103", page.Items[0].RoomNumber);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmptyPage_WhenNoRoomsExist()
    {
        await using var fixture = new RoomTestFixture();

        var page = await fixture.Rooms.GetPagedAsync();

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal(0, page.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByFloor()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101", floor: 1);
        fixture.AddRoom("201", floor: 2);

        var page = await fixture.Rooms.GetPagedAsync(floor: 2);

        Assert.Single(page.Items);
        Assert.Equal(2, page.Items[0].Floor);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByTypeAndPriceRange()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101", type: "Standard", pricePerDay: 100);
        fixture.AddRoom("201", type: "Deluxe", pricePerDay: 250);
        fixture.AddRoom("301", type: "Deluxe", pricePerDay: 350);

        var page = await fixture.Rooms.GetPagedAsync(roomType: "Deluxe", 
            minPrice: 200, maxPrice: 300);

        Assert.Single(page.Items);
        Assert.Equal("201", page.Items[0].RoomNumber);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnAllRoomsWithoutFilters()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101");
        fixture.AddRoom("102");

        var page = await fixture.Rooms.GetPagedAsync();

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldSupportZeroAndNegativePriceBoundsWithoutHttpFailure()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101", pricePerDay: 0);
        fixture.AddRoom("102", pricePerDay: 100);

        var zeroMinimum = await fixture.Rooms.GetPagedAsync(minPrice: 0);
        var zeroMaximum = await fixture.Rooms.GetPagedAsync(maxPrice: 0);
        var negativeMinimum = await fixture.Rooms.GetPagedAsync(minPrice: -1);
        var negativeMaximum = await fixture.Rooms.GetPagedAsync(maxPrice: -1);

        Assert.Equal(2, zeroMinimum.TotalCount);
        Assert.Single(zeroMaximum.Items);
        Assert.Equal("101", zeroMaximum.Items[0].RoomNumber);
        Assert.Equal(2, negativeMinimum.TotalCount);
        Assert.Empty(negativeMaximum.Items);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnEmpty_WhenMinimumPriceExceedsMaximumPrice()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101", pricePerDay: 100);

        var page = await fixture.Rooms.GetPagedAsync(minPrice: 200, maxPrice: 100);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldNormalizeInvalidPageValues()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom();

        var page = await fixture.Rooms.GetPagedAsync(pageNumber: 0, pageSize: 0);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(30, page.PageSize);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRoom_WhenDataIsValid()
    {
        await using var fixture = new RoomTestFixture();
        var command = new CreateRoomCommand("205", 2, "Deluxe", "Large room", 
            250, 3, 1, RoomOperationalStatus.Clean);

        var rooms = await fixture.Rooms.CreateAsync(command);
        var saved = await fixture.DbContext.Rooms.SingleAsync(room => 
        room.RoomNumber == "205");

        Assert.Single(rooms);
        Assert.Equal("205", saved.RoomNumber);
        Assert.Equal(2, saved.Floor);
        Assert.Equal("Deluxe", saved.Type);
        Assert.Equal(250, saved.PricePerDay);
        Assert.Equal(3, saved.Capacity);
        Assert.Equal(RoomOperationalStatus.Clean, saved.OperationalStatus);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnEmptyAndNotSave_WhenRoomNumberIsDuplicate()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101");

        var rooms = await fixture.Rooms.CreateAsync(new CreateRoomCommand(
            "101", 2, "Deluxe", "Duplicate", 200, 2, 1, RoomOperationalStatus.Clean));

        Assert.Empty(rooms);
        Assert.Equal(1, await fixture.DbContext.Rooms.CountAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_ShouldThrow_WhenRoomNumberIsEmpty(string roomNumber)
    {
        await using var fixture = new RoomTestFixture();

        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Rooms.
            CreateAsync(new CreateRoomCommand(
            roomNumber, 1, "Standard", "Test", 100, 1, 1, RoomOperationalStatus.Clean)));

        Assert.Empty(fixture.DbContext.Rooms);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenPriceIsNegative()
    {
        await using var fixture = new RoomTestFixture();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            fixture.Rooms.CreateAsync(new CreateRoomCommand(
            "101", 1, "Standard", "Test", -1, 1, 1, RoomOperationalStatus.Clean)));

        Assert.Empty(fixture.DbContext.Rooms);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCapacityIsZeroOrNegative()
    {
        await using var fixture = new RoomTestFixture();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
            fixture.Rooms.CreateAsync(new CreateRoomCommand("101", 1, "Standard", 
            "Test", 100, 0, 1, RoomOperationalStatus.Clean)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.
            Rooms.CreateAsync(new CreateRoomCommand("102", 1, "Standard", 
            "Test", 100, -1, 1, RoomOperationalStatus.Clean)));

        Assert.Empty(fixture.DbContext.Rooms);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRoom_WhenDataIsValid()
    {
        await using var fixture = new RoomTestFixture();
        var room = fixture.AddRoom();

        var updated = await fixture.Rooms.UpdateAsync(new UpdateRoomCommand(
            room.Id, "202", 2, "Deluxe", "Updated", 300, 4, 2, 
            RoomOperationalStatus.Cleaning));
        var saved = await fixture.DbContext.Rooms.FindAsync(room.Id);

        Assert.True(updated);
        Assert.Equal("202", saved!.RoomNumber);
        Assert.Equal(2, saved.Floor);
        Assert.Equal("Deluxe", saved.Type);
        Assert.Equal(300, saved.PricePerDay);
        Assert.Equal(4, saved.Capacity);
        Assert.Equal(RoomOperationalStatus.Cleaning, saved.OperationalStatus);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenRoomDoesNotExist()
    {
        await using var fixture = new RoomTestFixture();

        var updated = await fixture.Rooms.UpdateAsync(new UpdateRoomCommand(
            9999, "202", 2, "Deluxe", "Updated", 300, 4, 2, RoomOperationalStatus.Clean));

        Assert.False(updated);
        Assert.Empty(fixture.DbContext.Rooms);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalseAndNotChange_WhenRoomNumberIsDuplicate()
    {
        await using var fixture = new RoomTestFixture();
        var first = fixture.AddRoom("101");
        var second = fixture.AddRoom("102");

        var updated = await fixture.Rooms.UpdateAsync(new UpdateRoomCommand(
            second.Id, first.RoomNumber, 2, "Deluxe", "Updated", 300, 4, 
            2, RoomOperationalStatus.Clean));
        var saved = await fixture.DbContext.Rooms.FindAsync(second.Id);

        Assert.False(updated);
        Assert.Equal("102", saved!.RoomNumber);
    }

    [Fact]
    public async Task ChangeOperationalStatusAsync_ShouldPersistStatus()
    {
        await using var fixture = new RoomTestFixture();
        var room = fixture.AddRoom();

        var changed = await fixture.Rooms.ChangeOperationalStatusAsync
            (room.Id, RoomOperationalStatus.InMaintenance);

        Assert.True(changed);
        Assert.Equal(RoomOperationalStatus.InMaintenance, 
            (await fixture.DbContext.Rooms.FindAsync(room.Id))!.OperationalStatus);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenRoomDoesNotExist()
    {
        await using var fixture = new RoomTestFixture();

        var deleted = await fixture.Rooms.DeleteAsync(9999);

        Assert.False(deleted);
    }

    [Theory]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.CheckedIn)]
    public async Task DeleteAsync_ShouldReject_WhenRoomHasActiveReservation(ReservationStatus status)
    {
        await using var fixture = new RoomTestFixture();
        var room = fixture.AddRoom();
        var guest = fixture.AddGuest();
        fixture.AddReservation(guest, room, status: status);

        var deleted = await fixture.Rooms.DeleteAsync(room.Id);

        Assert.False(deleted);
        Assert.NotNull(await fixture.DbContext.Rooms.FindAsync(room.Id));
        Assert.Single(fixture.DbContext.Reservations);
    }

    [Theory]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.CheckedOut)]
    public async Task DeleteAsync_ShouldDeleteRoomAndTerminalReservations(ReservationStatus status)
    {
        await using var fixture = new RoomTestFixture();
        var room = fixture.AddRoom();
        var guest = fixture.AddGuest();
        fixture.AddReservation(guest, room, status: status);

        var deleted = await fixture.Rooms.DeleteAsync(room.Id);

        Assert.True(deleted);
        Assert.Null(await fixture.DbContext.Rooms.FindAsync(room.Id));
        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Fact]
    public async Task GetPeriodStatusesAsync_ShouldAllowOnlyCleanUnoccupiedRoomToBook()
    {
        await using var fixture = new RoomTestFixture();
        var clean = fixture.AddRoom("101", status: RoomOperationalStatus.Clean);
        var cleaning = fixture.AddRoom("102", status: RoomOperationalStatus.Cleaning);
        var maintenance = fixture.AddRoom("103", status: RoomOperationalStatus.InMaintenance);
        var guest = fixture.AddGuest();
        fixture.AddReservation(guest, clean, new DateOnly(2030, 1, 10), 
            new DateOnly(2030, 1, 12));

        var result = await fixture.Rooms.GetPeriodStatusesAsync
            (new DateOnly(2030, 1, 10), new DateOnly(2030, 1, 12));

        Assert.False(result.Items.Single(room => room.Id == clean.Id).CanBook);
        Assert.False(result.Items.Single(room => room.Id == cleaning.Id).CanBook);
        Assert.False(result.Items.Single(room => room.Id == maintenance.Id).CanBook);
    }

    [Fact]
    public async Task GetPeriodStatusesAsync_ShouldReturnAvailableCleanRoom_WhenReservationIsOutsidePeriod()
    {
        await using var fixture = new RoomTestFixture();
        var room = fixture.AddRoom();
        var guest = fixture.AddGuest();
        fixture.AddReservation(guest, room, new DateOnly(2030, 1, 10), 
            new DateOnly(2030, 1, 12));

        var result = await fixture.Rooms.GetPeriodStatusesAsync(new DateOnly
            (2030, 1, 12), new DateOnly(2030, 1, 14));

        Assert.True(result.Items.Single(item => item.Id == room.Id).CanBook);
    }

    [Fact]
    public async Task GetPeriodStatusesAsync_ShouldRejectInvalidDateRange()
    {
        await using var fixture = new RoomTestFixture();

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>
            (() => fixture.Rooms.GetPeriodStatusesAsync(
            new DateOnly(2030, 1, 12), new DateOnly(2030, 1, 12)));

        Assert.Equal(PersistenceErrorCode.InvalidDateRange, exception.ErrorCode);
    }

    [Fact]
    public async Task GetPeriodStatusesAsync_ShouldFilterByCapacity()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom("101", capacity: 1);
        fixture.AddRoom("102", capacity: 3);

        var result = await fixture.Rooms.GetPeriodStatusesAsync(
            new DateOnly(2030, 2, 1), new DateOnly(2030, 2, 2), guestsCount: 2);

        Assert.Single(result.Items);
        Assert.Equal("102", result.Items[0].RoomNumber);
    }

    [Fact]
    public async Task Availability_ShouldUseConfirmedAndCheckedInReservationsOnly()
    {
        await using var fixture = new RoomTestFixture();
        var confirmedRoom = fixture.AddRoom("101");
        var pendingRoom = fixture.AddRoom("102");
        var guest = fixture.AddGuest();
        fixture.AddReservation(guest, confirmedRoom, status: ReservationStatus.Confirmed);
        fixture.AddReservation(guest, pendingRoom, status: ReservationStatus.Pending);
        var from = new DateOnly(2030, 1, 10);
        var to = new DateOnly(2030, 1, 12);

        var confirmedStatus = await fixture.Availability.GetStatusForRangeAsync
            (confirmedRoom.Id, from, to);
        var pendingStatus = await fixture.Availability.GetStatusForRangeAsync
            (pendingRoom.Id, from, to);

        Assert.Equal(RoomAvailabilityStatus.Occupied, confirmedStatus);
        Assert.Equal(RoomAvailabilityStatus.Available, pendingStatus);
    }

    [Fact]
    public async Task Availability_ShouldRejectInvalidRange()
    {
        await using var fixture = new RoomTestFixture();

        await Assert.ThrowsAsync<PersistenceOperationException>(() => 
        fixture.Availability.GetStatusForRangeAsync(
            1, new DateOnly(2030, 1, 2), new DateOnly(2030, 1, 2)));
    }
}
