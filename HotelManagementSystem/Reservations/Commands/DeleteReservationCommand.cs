using FluentValidation;
using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record DeleteReservationCommand(int Id) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<DeleteReservationCommand>
    {
        public Validator() => RuleFor(x => x.Id).GreaterThan(0);
    }
}

