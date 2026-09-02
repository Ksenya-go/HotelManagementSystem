using FluentValidation;
using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Commands;

public sealed record DeleteRoomTypeCommand(int Id) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<DeleteRoomTypeCommand>
    {
        public Validator() => RuleFor(command => command.Id).GreaterThan(0);
    }
}
