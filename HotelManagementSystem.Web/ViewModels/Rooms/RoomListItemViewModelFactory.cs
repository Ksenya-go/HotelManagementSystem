using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Domain.Room;
using Microsoft.Extensions.Localization;

namespace HotelManagementSystem.Web.ViewModels.Rooms;

public sealed class RoomListItemViewModelFactory(
    IStringLocalizer<SharedResource> localizer)
{
    public RoomListItemViewModel Create(RoomDto room)
    {
        return new RoomListItemViewModel(
            room.Id,
            room.RoomNumber,
            room.Floor,
            room.Type,
            room.Description,
            room.PricePerDay,
            room.Capacity,
            room.RoomCount,
            room.OperationalStatus,
            room.Type,
            GetRoomStatusText(room.OperationalStatus));
    }

    private string GetRoomStatusText(RoomOperationalStatus status)
    {
        return localizer[$"RoomStatus_{status}"].Value;
    }
}