using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Application.RoomTypes;

public sealed record RoomDto(int Id, string RoomNumber, int Floor, string Type, string Description, 
    decimal PricePerDay, int Capacity, int RoomCount, IReadOnlyList<DateTime> BookedDates, 
    RoomOperationalStatus OperationalStatus, RoomAvailabilityStatus AvailabilityStatus);

