using HotelManagementSystem.Domain.Guest;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Persistence.EfCore.Rooms;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Tests.Rooms;

public sealed class RoomTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public ApplicationDbContext DbContext { get; }
    public RoomService Rooms { get; }
    public RoomAvailabilityService Availability { get; }

    public RoomTestFixture()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        DbContext = new ApplicationDbContext(options);
        DbContext.Database.EnsureCreated();
        Rooms = new RoomService(DbContext);
        Availability = new RoomAvailabilityService(DbContext);
    }

    public Room AddRoom(
        string roomNumber = "101",
        int floor = 1,
        string type = "Standard",
        decimal pricePerDay = 100,
        int capacity = 2,
        int roomCount = 1,
        RoomOperationalStatus status = RoomOperationalStatus.Clean)
    {
        var room = new Room(
            roomNumber,
            floor,
            type,
            "Test room",
            pricePerDay,
            capacity,
            roomCount,
            status);
        DbContext.Rooms.Add(room);
        DbContext.SaveChanges();
        return room;
    }

    public Guest AddGuest(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com")
    {
        var guest = new Guest(firstName, lastName, email, "+380501112233");
        DbContext.Guests.Add(guest);
        DbContext.SaveChanges();
        return guest;
    }

    public Reservation AddReservation(
        Guest guest,
        Room room,
        DateOnly? checkIn = null,
        DateOnly? checkOut = null,
        ReservationStatus status = ReservationStatus.Confirmed)
    {
        var reservation = new Reservation(
            guest.Id,
            room.Id,
            checkIn ?? new DateOnly(2030, 1, 10),
            checkOut ?? new DateOnly(2030, 1, 12),
            1);
        reservation.ChangeStatus(status);
        DbContext.Reservations.Add(reservation);
        DbContext.SaveChanges();
        return reservation;
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await connection.DisposeAsync();
    }
}
