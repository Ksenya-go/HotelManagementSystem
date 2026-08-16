using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Web.ViewModels.Rooms;

public sealed record RoomListItemViewModel(
    int Id,
    string RoomNumber,
    int Floor,
    string Type,
    string Description,
    decimal PricePerDay,
    int Capacity,
    int RoomCount,
    RoomOperationalStatus OperationalStatus,
    string DisplayType,
    string DisplayStatus)
{
}

