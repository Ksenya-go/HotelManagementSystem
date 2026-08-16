using FluentValidation.TestHelper;
using HotelManagementSystem.Application.Guests.Commands;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Application.RoomTypes.Commands;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Application.SystemSettings.Commands;
using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Guests;
using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Persistence.EfCore.Rooms;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Web.Tests.Admin;
using HotelManagementSystem.Web.Tests.Rooms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelManagementSystem.Web.Tests;

public sealed class EdgeCasesTests
{
    [Fact]
    public async Task CreateReservation_WhenGuestsEqualCapacity_ShouldSucceed()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom(capacity: 2);

        var result = await fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, new DateOnly(2030, 2, 1), 
            new DateOnly(2030, 2, 2), 2));

        Assert.Equal(2, result.GuestsCount);
        Assert.Single(fixture.DbContext.Reservations);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task CreateReservation_WhenGuestsCountIsNotPositive_ShouldFailWithoutSaving
        (int guestsCount)
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => fixture.Reservations.
        CreateAsync(new CreateReservationCommand(guest.Id, room.Id, new DateOnly
        (2030, 2, 1), new DateOnly(2030, 2, 2), guestsCount)));

        Assert.Empty(fixture.DbContext.Reservations);
    }

    [Fact]
    public async Task CreateReservation_WhenCheckOutIsTheNextDay_ShouldSucceed()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();

        var result = await fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, new DateOnly(2030, 2, 1), 
            new DateOnly(2030, 2, 2), 1));

        Assert.Equal(new DateOnly(2030, 2, 2), result.CheckOut);
    }

    [Theory]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.CheckedOut)]
    public async Task CreateReservation_WhenNewStayStartsOnExistingCheckOut_ShouldFollowActualStatusRule
        (ReservationStatus status)
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        fixture.AddReservation(guest, room, new DateOnly(2030, 2, 1), new DateOnly
            (2030, 2, 3), status: status);

        var result = await fixture.Reservations.CreateAsync(
            new CreateReservationCommand(guest.Id, room.Id, new DateOnly(2030, 2, 3), 
            new DateOnly(2030, 2, 5), 1));

        Assert.Equal(ReservationStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateReservation_WhenNewStayOverlapsConfirmedReservationByOneDay_ShouldFail()
    {
        await using var fixture = new ReservationTestFixture();
        var guest = fixture.AddGuest();
        var room = fixture.AddRoom();
        fixture.AddReservation(guest, room, new DateOnly(2030, 2, 1), new DateOnly
            (2030, 2, 3), status: ReservationStatus.Confirmed);

        var exception = await Assert.ThrowsAsync<PersistenceOperationException>(() 
            => fixture.Reservations.CreateAsync(new CreateReservationCommand(guest.Id, 
            room.Id, new DateOnly(2030, 2, 2), new DateOnly(2030, 2, 4), 1)));

        Assert.Equal(PersistenceErrorCode.RoomAlreadyReserved, exception.ErrorCode);
        Assert.Single(fixture.DbContext.Reservations);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task CreateRoom_WhenCapacityIsNotPositive_ShouldFail(int capacity)
    {
        await using var fixture = new RoomTestFixture();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
        fixture.Rooms.CreateAsync(new CreateRoomCommand("101", 1, "Standard", "Test", 
        100, capacity, 1, RoomOperationalStatus.Clean)));

        Assert.Empty(fixture.DbContext.Rooms);
    }

    [Fact]
    public async Task CreateRoom_WhenPriceIsZero_ShouldSucceed()
    {
        await using var fixture = new RoomTestFixture();

        var result = await fixture.Rooms.CreateAsync(
            new CreateRoomCommand("101", 1, "Standard", "Test", 0, 1, 1, 
            RoomOperationalStatus.Clean));

        Assert.Single(result);
        Assert.Equal(0, result[0].PricePerDay);
    }

    [Fact]
    public async Task CreateRoom_WhenFloorIsZero_ShouldFailWithoutSaving()
    {
        await using var fixture = new RoomTestFixture();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => 
        fixture.Rooms.CreateAsync(new CreateRoomCommand("101", 0, "Standard", "Test", 
        100, 1, 1, RoomOperationalStatus.Clean)));

        Assert.Empty(fixture.DbContext.Rooms);
    }

    [Fact]
    public async Task GetRooms_WhenMinimumPriceExceedsMaximumPrice_ShouldReturnEmptyPage()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom(pricePerDay: 100);

        var result = await fixture.Rooms.GetPagedAsync(minPrice: 101, maxPrice: 100);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetRooms_WhenPageIsNegativeAndPageSizeIsZero_ShouldNormalizeValues()
    {
        await using var fixture = new RoomTestFixture();
        fixture.AddRoom();

        var result = await fixture.Rooms.GetPagedAsync(pageNumber: -1, pageSize: 0);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(30, result.PageSize);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task CreateRoom_WhenRoomNumberIsAtMostTwentyCharacters_ShouldUseActualDomainLimit()
    {
        await using var fixture = new RoomTestFixture();
        var roomNumber = new string('R', 20);

        var result = await fixture.Rooms.CreateAsync(
            new CreateRoomCommand(roomNumber, 1, "Standard", "Test", 100, 1, 1, 
            RoomOperationalStatus.Clean));

        Assert.Single(result);
        Assert.Equal(roomNumber, result[0].RoomNumber);
    }

    [Fact]
    public async Task CreateGuest_WhenValidatorAcceptsMaximumNameAndPhoneLengths_ShouldPersist()
    {
        await using var fixture = new ReservationTestFixture();
        var service = new GuestService(fixture.DbContext);
        var command = new CreateGuestCommand(new string('A', 100), new string('B', 100), "boundary@example.com", new string('1', 30));

        var result = await service.CreateAsync(command);

        Assert.Equal(201, result.FullName.Length);
        Assert.Equal(30, result.Phone.Length);
        Assert.Single(fixture.DbContext.Guests);
    }

    [Fact]
    public async Task CreateGuest_WhenDomainRequiredNameIsBlank_ShouldFailWithoutSaving()
    {
        await using var fixture = new ReservationTestFixture();
        var service = new GuestService(fixture.DbContext);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            new CreateGuestCommand(" ", "Doe", "guest@example.com", "123")));

        Assert.Empty(fixture.DbContext.Guests);
    }

    [Fact]
    public async Task CreateGuest_WhenEmailHasNoEmailFormat_ShouldBeRejectedByValidator()
    {
        var validator = new CreateGuestCommand.Validator();

        var result = await validator.TestValidateAsync(
            new CreateGuestCommand("John", "Doe", "not-an-email", "123"));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Fact]
    public async Task CreateGuest_WhenPhoneIsThirtyCharacters_ShouldPassValidator()
    {
        var validator = new CreateGuestCommand.Validator();

        var result = await validator.TestValidateAsync(
            new CreateGuestCommand("John", "Doe", "john@example.com", 
            new string('1', 30)));

        result.ShouldNotHaveValidationErrorFor(command => command.Phone);
    }

    [Fact]
    public async Task CreateRoomType_WhenBasePriceIsZeroAndMaxGuestsIsOne_ShouldSucceed()
    {
        await using var fixture = new RoomTypeEdgeFixture();

        var result = await fixture.Service.CreateAsync(new CreateRoomTypeCommand
            ("Single", "", 0, 1));

        Assert.Equal(0, result.BasePrice);
        Assert.Equal(1, result.MaxGuests);
        Assert.Empty(result.Description);
    }

    [Fact]
    public async Task CreateRoomType_WhenMaxGuestsIsZero_ShouldBeRejectedByValidator()
    {
        var validator = new CreateRoomTypeCommand.Validator();

        var result = await validator.TestValidateAsync(new CreateRoomTypeCommand
            ("Single", "", 0, 0));

        result.ShouldHaveValidationErrorFor(command => command.MaxGuests);
    }

    [Fact]
    public async Task CreateRoomType_WhenNameIsOneHundredCharacters_ShouldPassValidator()
    {
        var validator = new CreateRoomTypeCommand.Validator();

        var result = await validator.TestValidateAsync(
            new CreateRoomTypeCommand(new string('T', 100), new string('D', 500), 0, 1));

        result.ShouldNotHaveValidationErrorFor(command => command.Name);
        result.ShouldNotHaveValidationErrorFor(command => command.Description);
    }

    [Fact]
    public async Task CreateRoomType_WhenBasePriceIsNegative_ShouldBeRejectedByValidator()
    {
        var validator = new CreateRoomTypeCommand.Validator();

        var result = await validator.TestValidateAsync(new CreateRoomTypeCommand
            ("Single", "", -0.01m, 1));

        result.ShouldHaveValidationErrorFor(command => command.BasePrice);
    }

    [Fact]
    public async Task UpdateSetting_WhenValueHasFiveHundredCharacters_ShouldPassValidator()
    {
        var validator = new UpdateSystemSettingCommand.Validator();

        var result = await validator.TestValidateAsync(
            new UpdateSystemSettingCommand(1, new string('x', 500)));

        result.ShouldNotHaveValidationErrorFor(command => command.Value);
    }

    [Fact]
    public async Task UpdateSetting_WhenValueIsEmpty_ShouldBeRejectedByValidator()
    {
        var validator = new UpdateSystemSettingCommand.Validator();

        var result = await validator.TestValidateAsync(new UpdateSystemSettingCommand
            (1, ""));

        result.ShouldHaveValidationErrorFor(command => command.Value);
    }

    [Fact]
    public async Task AdminUser_WhenEmailIsDuplicate_ShouldNotCreateSecondIdentityUser()
    {
        await using var fixture = new AdminUsersTestFixture();
        await fixture.CreateUserAsync("edge-user@example.com");
        await using var scope = fixture.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.
            Identity.UserManager<ApplicationUser>>();
        var duplicate = new ApplicationUser { UserName = "edge-user@example.com", 
            Email = "edge-user@example.com", FullName = "Duplicate" };

        var result = await manager.CreateAsync(duplicate, "Password123");

        Assert.False(result.Succeeded);
        Assert.Equal(1, await manager.Users.CountAsync(user => user.Email == 
        "edge-user@example.com"));
    }

    [Fact]
    public async Task AdminUser_WhenPasswordHasEightCharacters_ShouldBeAcceptedByIdentity()
    {
        await using var fixture = new AdminUsersTestFixture();
        var user = await fixture.CreateUserAsync("minimum-password@example.com", 
            "Pass1234");

        Assert.NotNull(await fixture.FindUserAsync(user.Id));
    }

    [Fact]
    public async Task AdminUser_WhenFullNameExceedsViewModelLimit_ShouldHaveValidationError()
    {
        var model = new HotelManagementSystem.Web.ViewModels.Admin.
            CreateEmployeeViewModel
        {
            FullName = new string('N', 121),
            Email = "long-name@example.com",
            Password = "Password123",
            Role = "Employee"
        };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var valid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject
            (model, context, errors, true);

        Assert.False(valid);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof
            (model.FullName)));
    }

    private sealed class RoomTypeEdgeFixture : IAsyncDisposable
    {
        private readonly Microsoft.Data.Sqlite.SqliteConnection connection = 
            new("Data Source=:memory:");
        public ApplicationDbContext DbContext { get; }
        public RoomTypeService Service { get; }

        public RoomTypeEdgeFixture()
        {
            connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite
                (connection).Options;
            DbContext = new ApplicationDbContext(options);
            DbContext.Database.EnsureCreated();
            Service = new RoomTypeService(DbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
