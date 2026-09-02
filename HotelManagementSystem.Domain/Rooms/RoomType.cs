namespace HotelManagementSystem.Domain.Rooms;

public sealed class RoomType
{
    private RoomType()
    {
    }

    public RoomType(
        string name,
        string description,
        decimal basePrice,
        int maxGuests)
    {
        Update(name, description, basePrice, maxGuests);
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal BasePrice { get; private set; }

    public int MaxGuests { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void Update(
        string name,
        string description,
        decimal basePrice,
        int maxGuests)
    {
        Validate(name, basePrice, maxGuests);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        BasePrice = basePrice;
        MaxGuests = maxGuests;
    }

    private static void Validate(string name, decimal basePrice, int maxGuests)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                RoomValidationMessages.RoomTypeNameRequired,
                nameof(name));
        }

        if (basePrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(basePrice),
                RoomValidationMessages.RoomTypeBasePriceNegative);
        }

        if (maxGuests < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxGuests),
                RoomValidationMessages.RoomTypeMaxGuestsTooLow);
        }
    }
}