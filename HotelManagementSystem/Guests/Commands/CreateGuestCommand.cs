using FluentValidation;

using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.Guests.Commands;

public sealed record CreateGuestCommand(string? FirstName, string? LastName, string? Email, 
    string? Phone) : ICommand<Result<GuestDto>>
{
    public sealed class Validator : AbstractValidator<CreateGuestCommand>
    {
        public Validator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        }
    }
}

