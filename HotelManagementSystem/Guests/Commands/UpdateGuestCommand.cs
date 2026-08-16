using FluentValidation;
using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Guests.Commands;

public sealed record UpdateGuestCommand(int Id, string? FirstName, string? LastName, 
    string? Email, string? Phone) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<UpdateGuestCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        }
    }
}

