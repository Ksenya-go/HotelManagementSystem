namespace HotelManagementSystem.Application.RoomTypes;

public sealed record RoomTypeDto(
    int Id,
    string Name,
    string Description,
    decimal BasePrice,
    int MaxGuests);


