using FluentValidation;
using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record CheckInReservationCommand(int Id) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<CheckInReservationCommand>
    {
        public Validator() => RuleFor(x => x.Id).GreaterThan(0);
    }
}

