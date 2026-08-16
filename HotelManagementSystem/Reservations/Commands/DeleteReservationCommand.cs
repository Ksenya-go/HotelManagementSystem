using FluentValidation;
using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record DeleteReservationCommand(int Id) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<DeleteReservationCommand>
    {
        public Validator() => RuleFor(x => x.Id).GreaterThan(0);
    }
}

