using HotelManagementSystem.Application.Guests;
using HotelManagementSystem.Application.Guests.Commands;

namespace HotelManagementSystem.Application.Services;

public interface IGuestService
{
    Task<IReadOnlyList<GuestDto>> GetAllAsync(string? query = null, CancellationToken cancellationToken = default);
    Task<GuestDto> CreateAsync(CreateGuestCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateGuestCommand command, CancellationToken cancellationToken = default);
}

