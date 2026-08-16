using HotelManagementSystem.Domain.Entities;
using HotelManagementSystem.Domain.Guest;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Domain.Room;
using GuestEntity = HotelManagementSystem.Domain.Guest.Guest;
using RoomEntity = HotelManagementSystem.Domain.Room.Room;
using ReservationEntity = HotelManagementSystem.Domain.Reservation.Reservation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace HotelManagementSystem.Persistence.EfCore.Identity;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<GuestEntity> Guests => Set<GuestEntity>();
    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();
    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RoomType>(entity =>
        {
            entity.Property(roomType => roomType.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(roomType => roomType.Name).IsUnique();
            entity.Property(roomType => roomType.Description).HasMaxLength(500);
            entity.Property(roomType => roomType.BasePrice).HasPrecision(12, 2);
        });

        builder.Entity<GuestEntity>(entity =>
        {
            entity.Property(guest => guest.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(guest => guest.LastName).HasMaxLength(100).IsRequired();
            entity.Property(guest => guest.Email).HasMaxLength(200).IsRequired();
            entity.Property(guest => guest.Phone).HasMaxLength(40);
        });

        builder.Entity<RoomEntity>(entity =>
        {
            var bookedDatesComparer = new ValueComparer<List<DateTime>>(
                (left, right) => left!.SequenceEqual(right!),
                dates => dates.Aggregate(
                    0,
                    (hash, date) => HashCode.Combine(
                        hash,
                        date.GetHashCode())),
                dates => dates.ToList());

            entity.Property(room => room.RoomNumber)
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(room => room.RoomNumber).IsUnique();
            entity.Property(room => room.Type)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(room => room.Description).HasMaxLength(500);
            entity.Property(room => room.PricePerDay).HasPrecision(12, 2);
            entity.Property(room => room.Capacity).HasDefaultValue(1);
            entity.Property(room => room.RoomCount).HasDefaultValue(1);
            entity.Property(room => room.BookedDates)
                .HasConversion(
                    dates => JsonSerializer.Serialize(
                        dates,
                        (JsonSerializerOptions?)null),
                    value => JsonSerializer.Deserialize<List<DateTime>>(
                        value,
                        (JsonSerializerOptions?)null) ?? new List<DateTime>())
                .Metadata.SetValueComparer(bookedDatesComparer);
            entity.Property(room => room.BookedDates)
                .HasColumnType("jsonb");
            entity.Property(room => room.OperationalStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasColumnName("Status");
        });

        builder.Entity<ReservationEntity>(entity =>
        {
            entity.Property(reservation => reservation.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
            entity.HasOne(reservation => reservation.Guest)
                .WithMany(guest => guest.Reservations)
                .HasForeignKey(reservation => reservation.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(reservation => reservation.Room)
                .WithMany()
                .HasForeignKey(reservation => reservation.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SystemSetting>(entity =>
        {
            entity.Property(setting => setting.Key).HasMaxLength(100).IsRequired();
            entity.HasIndex(setting => setting.Key).IsUnique();
            entity.Property(setting => setting.Value).HasMaxLength(2000).IsRequired();
            entity.Property(setting => setting.Description).HasMaxLength(300);
        });
    }
}
