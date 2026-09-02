using FluentValidation;
using HotelManagementSystem.Domain.Reservations;
using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record ChangeReservationStatusCommand(int Id, ReservationStatus Status) : 
    ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<ChangeReservationStatusCommand>
    {
        public Validator() => RuleFor(x => x.Id).GreaterThan(0);
    }
}

