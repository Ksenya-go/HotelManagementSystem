namespace HotelManagementSystem.Application.Common.Cqrs.Results;

public readonly record struct Unit
{
    public static Unit Value { get; } = new();
}

