using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Application.Common.Presentation;

public static class RoomStatusText
{
    public static string GetOperationalStatus(
        RoomOperationalStatus operationalStatus)
    {
        return operationalStatus switch
        {
            RoomOperationalStatus.InMaintenance => "На обслуговуванні",
            RoomOperationalStatus.Cleaning => "Потребує прибирання",
            _ => "Доступний"
        };
    }

    public static string GetAvailabilityStatus(bool isOccupied)
    {
        return isOccupied ? "Зайнятий" : "Доступний";
    }
}
