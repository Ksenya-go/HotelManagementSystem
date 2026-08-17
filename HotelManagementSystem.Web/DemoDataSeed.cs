using HotelManagementSystem.Domain.Guest;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web;

public static class DemoDataSeed
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (!configuration.GetValue("Seed:IncludeDemoData", true))
        {
            return;
        }

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        if (await dbContext.Rooms.AnyAsync() || await dbContext.Reservations.AnyAsync())
        {
            return;
        }

        var roomTypes = new[]
        {
            new RoomType(
                "Стандарт",
                "Номер із двома окремими ліжками",
                500,
                2),
            new RoomType(
                "Напівлюкс",
                "Просторий номер зі спальним місцем та окремою зоною відпочинку",
                1000,
                3),
            new RoomType(
                "Люкс",
                "Просторий номер із окремою спальнею та вітальнею",
                1500,
                3)
        };

        var rooms = new[]
        {
            new Room("102", 1, "Стандарт", "Номер із двома окремими ліжками", 500, 2, 1, RoomOperationalStatus.Clean),
            new Room("109", 1, "Напівлюкс", "Просторий номер зі спальним місцем та окремою зоною відпочинку", 1000, 3, 2, RoomOperationalStatus.Clean),
            new Room("202", 2, "Стандарт", "Номер із двома окремими ліжками", 500, 2, 1, RoomOperationalStatus.Clean),
            new Room("303", 3, "Напівлюкс", "Просторий номер зі спальним місцем та окремою зоною відпочинку", 1000, 3, 2, RoomOperationalStatus.Clean),
            new Room("406", 4, "Стандарт", "Номер із двома окремими ліжками", 500, 2, 1, RoomOperationalStatus.Clean),
            new Room("415", 4, "Люкс", "Просторий номер із окремою спальнею та вітальнею", 1500, 3, 2, RoomOperationalStatus.InMaintenance),
            new Room("555", 5, "Стандарт", "Номер із двома окремими ліжками", 500, 2, 1, RoomOperationalStatus.Cleaning),
            new Room("601", 6, "Напівлюкс", "Просторий номер зі спальним місцем та окремою зоною відпочинку", 1000, 3, 2, RoomOperationalStatus.Clean),
            new Room("701", 7, "Напівлюкс", "Просторий номер зі спальним місцем та окремою зоною відпочинку", 1000, 3, 2, RoomOperationalStatus.Clean),
            new Room("800", 8, "Люкс", "Просторий номер із окремою спальнею та вітальнею", 1500, 3, 2, RoomOperationalStatus.Clean),
            new Room("901", 9, "Люкс", "Просторий номер із окремою спальнею та вітальнею", 1500, 3, 2, RoomOperationalStatus.Clean)
        };

        var guests = new[]
        {
            new Guest("Дмитро", "Шевченко", "dmytro.shevchenko@gmail.com", "+380671234501"),
            new Guest("Максим", "Романенко", "maksym.romanenko@gmail.com", "+380671234502"),
            new Guest("Софія", "Ткаченко", "sofia.tkachenko@gmail.com", "+380671234503"),
            new Guest("Катерина", "Іваненко", "kateryna.ivanenko@gmail.com", "+380671234504"),
            new Guest("Марія", "Бондаренко", "maria.bondarenko@gmail.com", "+380671234505"),
            new Guest("Олена", "Кравченко", "olena.kravchenko@gmail.com", "+380671234506"),
            new Guest("Андрій", "Мельник", "andrii.melnyk@gmail.com", "+380671234507"),
            new Guest("Наталія", "Коваленко", "nataliia.kovalenko@gmail.com", "+380671234508")
        };

        dbContext.RoomTypes.AddRange(roomTypes);
        dbContext.Rooms.AddRange(rooms);
        dbContext.Guests.AddRange(guests);
        await dbContext.SaveChangesAsync();

        var reservations = new[]
        {
            CreateReservation(guests[0], rooms[8], new DateOnly(2026, 9, 22), new DateOnly(2026, 9, 26), 2, ReservationStatus.Confirmed),
            CreateReservation(guests[1], rooms[10], new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13), 2, ReservationStatus.Pending),
            CreateReservation(guests[2], rooms[2], new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 6), 1, ReservationStatus.Confirmed),
            CreateReservation(guests[3], rooms[10], new DateOnly(2026, 8, 24), new DateOnly(2026, 8, 29), 2, ReservationStatus.CheckedIn),
            CreateReservation(guests[4], rooms[9], new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13), 3, ReservationStatus.Cancelled),
            CreateReservation(guests[5], rooms[2], new DateOnly(2026, 8, 4), new DateOnly(2026, 8, 9), 1, ReservationStatus.CheckedOut),
            CreateReservation(guests[6],rooms[7],new DateOnly(2026, 8, 3),new DateOnly(2026, 8, 5),2,ReservationStatus.CheckedOut),
            CreateReservation(guests[7], rooms[1], new DateOnly(2026, 7, 18), new DateOnly(2026, 7, 21), 2, ReservationStatus.Confirmed)
        };

        dbContext.Reservations.AddRange(reservations);
        await dbContext.SaveChangesAsync();
    }

    private static Reservation CreateReservation(
        Guest guest,
        Room room,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestsCount,
        ReservationStatus status)
    {
        var reservation = new Reservation(
            guest.Id,
            room.Id,
            checkIn,
            checkOut,
            guestsCount);

        reservation.ChangeStatus(status);
        return reservation;
    }
}
