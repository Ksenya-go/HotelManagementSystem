using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Application.RoomTypes.Commands;
using HotelManagementSystem.Domain.Room;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomTypeEntity = HotelManagementSystem.Domain.Room.RoomType;

namespace HotelManagementSystem.Persistence.EfCore.Rooms;

public sealed class RoomTypeService(ApplicationDbContext dbContext)
    : IRoomTypeService
{
    public async Task<IReadOnlyList<RoomTypeDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.RoomTypes
            .AsNoTracking()
            .OrderBy(roomType => roomType.Name)
            .Select(roomType => new RoomTypeDto(
                roomType.Id,
                roomType.Name,
                roomType.Description,
                roomType.BasePrice,
                roomType.MaxGuests))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        UpdateRoomTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        var roomType = await dbContext.RoomTypes
            .SingleOrDefaultAsync(
                item => item.Id == command.Id,
                cancellationToken);

        if (roomType is null)
        {
            return false;
        }

        roomType.Update(
            command.Name,
            command.Description,
            command.BasePrice,
            command.MaxGuests);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<RoomTypeDto> CreateAsync(
        CreateRoomTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        var roomType = new RoomTypeEntity(
            command.Name,
            command.Description,
            command.BasePrice,
            command.MaxGuests);

        dbContext.RoomTypes.Add(roomType);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RoomTypeDto(
            roomType.Id,
            roomType.Name,
            roomType.Description,
            roomType.BasePrice,
            roomType.MaxGuests);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var roomType = await dbContext.RoomTypes
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (roomType is null)
        {
            return false;
        }

        dbContext.RoomTypes.Remove(roomType);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}