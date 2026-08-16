using HotelManagementSystem.Application.Guests;
using HotelManagementSystem.Application.Guests.Commands;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;
using GuestEntity = HotelManagementSystem.Domain.Guest.Guest;

namespace HotelManagementSystem.Persistence.EfCore.Guests;

public sealed class GuestService(ApplicationDbContext dbContext)
    : IGuestService
{
    public async Task<IReadOnlyList<GuestDto>> GetAllAsync(
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        var guests = dbContext.Guests
            .AsNoTracking()
            .Where(guest =>
                string.IsNullOrWhiteSpace(query) ||
                (guest.FirstName + " " + guest.LastName)
                    .ToLower()
                    .Contains(query!.ToLower()) ||
                guest.Email
                    .ToLower()
                    .Contains(query.ToLower()) ||
                guest.Phone.Contains(query))
            .OrderBy(guest => guest.LastName)
            .ThenBy(guest => guest.FirstName)
            .Select(guest => new GuestDto(
                guest.Id,
                guest.FirstName + " " + guest.LastName,
                guest.Email,
                guest.Phone));

        return await guests.ToListAsync(cancellationToken);
    }

    public async Task<GuestDto> CreateAsync(
        CreateGuestCommand command,
        CancellationToken cancellationToken = default)
    {
        var guest = new GuestEntity(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone);

        dbContext.Guests.Add(guest);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new GuestDto(
            guest.Id,
            guest.FirstName + " " + guest.LastName,
            guest.Email,
            guest.Phone);
    }

    public async Task<bool> UpdateAsync(
        UpdateGuestCommand command,
        CancellationToken cancellationToken = default)
    {
        var guest = await dbContext.Guests
            .SingleOrDefaultAsync(
                item => item.Id == command.Id,
                cancellationToken);

        if (guest is null)
        {
            return false;
        }

        guest.Update(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}