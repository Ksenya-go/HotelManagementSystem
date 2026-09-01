using FluentValidation;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record UpdateReservationCommand(int Id, int GuestId, DateOnly CheckIn, 
    DateOnly CheckOut, int GuestsCount) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<UpdateReservationCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.GuestId).GreaterThan(0);
            RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn);
            RuleFor(x => x.GuestsCount).GreaterThan(0);
        }
    }
}

