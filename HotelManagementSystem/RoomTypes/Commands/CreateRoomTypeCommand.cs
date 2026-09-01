using FluentValidation;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Commands;

public sealed record CreateRoomTypeCommand(
    string Name,
    string Description,
    decimal BasePrice,
    int MaxGuests) : ICommand<Result<RoomTypeDto>>
{
    public sealed class Validator : AbstractValidator<CreateRoomTypeCommand>
    {
        public Validator()
        {
            RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Description).MaximumLength(500);
            RuleFor(command => command.BasePrice).GreaterThanOrEqualTo(0);
            RuleFor(command => command.MaxGuests).GreaterThan(0);
        }
    }
}
