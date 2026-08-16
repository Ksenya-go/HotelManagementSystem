namespace HotelManagementSystem.Application.RoomTypes;

using HotelManagementSystem.Application.RoomTypes.Commands;

public interface IRoomTypeService
{
    Task<IReadOnlyList<RoomTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomTypeDto> CreateAsync(CreateRoomTypeCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateRoomTypeCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

