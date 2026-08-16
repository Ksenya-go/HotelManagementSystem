namespace HotelManagementSystem.Application.RoomTypes;

public sealed record RoomPeriodStatusDto(int Id, string RoomNumber, int Floor, string Type, 
    string Description, decimal PricePerDay, int Capacity, int RoomCount, string OperationalStatus, 
    string AvailabilityStatus, bool CanBook);

