namespace HotelManagementSystem.Domain.Room;

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
                "Номер кімнати є обов’язковим.",
                nameof(roomNumber));
        }

        if (floor < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(floor),
                "Поверх має бути щонайменше 1.");
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(
                "Тип кімнати є обов’язковим.",
                nameof(type));
        }

        if (pricePerDay < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pricePerDay),
                "Ціна за добу не може бути від’ємною.");
        }

        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Місткість номера має бути щонайменше 1.");
        }

        if (roomCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roomCount),
                "Кількість кімнат має бути щонайменше 1.");
        }
    }
}