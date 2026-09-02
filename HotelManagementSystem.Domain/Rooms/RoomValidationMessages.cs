namespace HotelManagementSystem.Domain.Rooms;

internal static class RoomValidationMessages
{
    public const string RoomNumberRequired = "Номер кімнати є обов'язковим.";
    public const string FloorTooLow = "Поверх має бути щонайменше 1.";
    public const string TypeRequired = "Тип кімнати є обов'язковим.";
    public const string PriceNegative = "Ціна за добу не може бути від'ємною.";
    public const string CapacityTooLow = "Місткість номера має бути щонайменше 1.";
    public const string RoomCountTooLow = "Кількість кімнат має бути щонайменше 1.";
}
