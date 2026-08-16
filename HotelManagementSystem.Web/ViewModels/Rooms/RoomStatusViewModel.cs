using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Web.ViewModels.Rooms;

public sealed class RoomStatusViewModel
{
    public int Id { get; set; }
    public RoomOperationalStatus OperationalStatus { get; set; }
}

