using HotelManagementSystem.Application.RoomTypes;

namespace HotelManagementSystem.Web.ViewModels.Admin;

public sealed class AdminRoomTypesViewModel
{
    public IReadOnlyList<RoomTypeDto> RoomTypes { get; init; } = [];
}
