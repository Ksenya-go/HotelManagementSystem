using FluentValidation;
using FluentResults;
using HotelManagementSystem.Application.RoomTypes;
using Mediator;
using HotelManagementSystem.Domain.Rooms;

namespace HotelManagementSystem.Application.Rooms.Commands;

public sealed record CreateRoomCommand(
    string RoomNumber,
    int Floor,
    string Type,
    string Description,
    decimal PricePerDay,
    int Capacity,
    int RoomCount,
    RoomOperationalStatus OperationalStatus) : ICommand<Result<IReadOnlyList<RoomDto>>>
{
    public sealed class Validator : AbstractValidator<CreateRoomCommand>
    {
        public Validator()
        {
            RuleFor(command => command.RoomNumber).NotEmpty().MaximumLength(20);
            RuleFor(command => command.Floor).GreaterThan(0);
            RuleFor(command => command.Type).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Description).MaximumLength(500);
            RuleFor(command => command.PricePerDay).GreaterThanOrEqualTo(0);
            RuleFor(command => command.Capacity).GreaterThan(0);
            RuleFor(command => command.RoomCount).GreaterThan(0);
        }
    }
}
