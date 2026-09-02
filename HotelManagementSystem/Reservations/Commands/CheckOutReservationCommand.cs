using FluentValidation;
using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record CheckOutReservationCommand(int Id) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<CheckOutReservationCommand>
    {
        public Validator() => RuleFor(x => x.Id).GreaterThan(0);
    }
}

