namespace HotelManagementSystem.Domain.Rooms;

public sealed class Room
{
    private Room()
    {
        BookedDates = [];
    }

    public Room(
        string roomNumber,
        int floor,
        string type,
        string description,
        decimal pricePerDay,
        int capacity,
        int roomCount,
        RoomOperationalStatus operationalStatus)
    {
        Validate(roomNumber, floor, type, pricePerDay, capacity, roomCount);

        RoomNumber = roomNumber.Trim();
        Floor = floor;
        Type = type.Trim();
        Description = description.Trim();
        PricePerDay = pricePerDay;
        Capacity = capacity;
        RoomCount = roomCount;
        OperationalStatus = operationalStatus;
        BookedDates = [];
    }

    public int Id { get; private set; }

    public string RoomNumber { get; private set; } = string.Empty;

    public int Floor { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal PricePerDay { get; private set; }

    public int Capacity { get; private set; } = 1;

    public int RoomCount { get; private set; } = 1;

    public List<DateTime> BookedDates { get; private set; } = [];

    public RoomOperationalStatus OperationalStatus { get; private set; }

    public void ChangeOperationalStatus(
        RoomOperationalStatus operationalStatus)
    {
        OperationalStatus = operationalStatus;
    }

    public void Update(
        string roomNumber,
        int floor,
        string type,
        string description,
        decimal pricePerDay,
        int capacity,
        int roomCount,
        RoomOperationalStatus operationalStatus)
    {
        Validate(roomNumber, floor, type, pricePerDay, capacity, roomCount);

        RoomNumber = roomNumber.Trim();
        Floor = floor;
        Type = type.Trim();
        Description = description.Trim();
        PricePerDay = pricePerDay;
        Capacity = capacity;
        RoomCount = roomCount;
        OperationalStatus = operationalStatus;
    }

    private static void Validate(
    string roomNumber,
    int floor,
    string type,
    decimal pricePerDay,
    int capacity,
    int roomCount)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            throw new ArgumentException(
                RoomValidationMessages.RoomNumberRequired,
                nameof(roomNumber));
        }

        if (floor < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(floor),
                RoomValidationMessages.FloorTooLow);
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(
                RoomValidationMessages.TypeRequired,
                nameof(type));
        }

        if (pricePerDay < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pricePerDay),
                RoomValidationMessages.PriceNegative);
        }

        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                RoomValidationMessages.CapacityTooLow);
        }

        if (roomCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roomCount),
                RoomValidationMessages.RoomCountTooLow);
        }
    }
}