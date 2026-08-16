using HotelManagementSystem.Domain.Guest;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Persistence.EfCore.Reservations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Tests;

public sealed class ReservationTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public ApplicationDbContext DbContext { get; }
    public ReservationService Reservations { get; }

    public ReservationTestFixture()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        DbContext = new ApplicationDbContext(options);
        DbContext.Database.EnsureCreated();
        Reservations = new ReservationService(DbContext);
    }

    public Guest AddGuest(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string phone = "+380501112233")
    {
        var guest = new Guest(firstName, lastName, email, phone);
        DbContext.Guests.Add(guest);
        DbContext.SaveChanges();
        return guest;
    }

    public Room AddRoom(
        string roomNumber = "101",
        int capacity = 2,
        RoomOperationalStatus status = RoomOperationalStatus.Clean)
    {
        var room = new Room(
            roomNumber,
            floor: 1,
            type: "Standard",
            description: "Test room",
            pricePerDay: 100,
            capacity,
            roomCount: 1,
            status);
        DbContext.Rooms.Add(room);
        DbContext.SaveChanges();
        return room;
    }

    public Reservation AddReservation(
        Guest guest,
        Room room,
        DateOnly? checkIn = null,
        DateOnly? checkOut = null,
        int guestsCount = 1,
        ReservationStatus status = ReservationStatus.Pending)
    {
        var reservation = new Reservation(
            guest.Id,
            room.Id,
            checkIn ?? new DateOnly(2030, 1, 10),
            checkOut ?? new DateOnly(2030, 1, 12),
            guestsCount);
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
