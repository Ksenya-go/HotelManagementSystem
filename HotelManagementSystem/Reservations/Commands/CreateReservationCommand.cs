using FluentValidation;
using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Reservations.Commands;

public sealed record CreateReservationCommand(int GuestId, int RoomId, DateOnly CheckIn, 
    DateOnly CheckOut, int GuestsCount) : ICommand<Result<ReservationDto>>
{
    public sealed class Validator : AbstractValidator<CreateReservationCommand>
    {
        public Validator()
        {
            RuleFor(x => x.GuestId).GreaterThan(0);
            RuleFor(x => x.RoomId).GreaterThan(0);
            RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn);
            RuleFor(x => x.GuestsCount).GreaterThan(0);
        }
    }
}

