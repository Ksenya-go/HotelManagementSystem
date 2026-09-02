using FluentValidation;
using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.SystemSettings.Commands;

public sealed record UpdateSystemSettingCommand(int Id, string Value) : ICommand<Result<Unit>>
{
    public sealed class Validator : AbstractValidator<UpdateSystemSettingCommand>
    {
        public Validator()
        {
            RuleFor(command => command.Id).GreaterThan(0);
            RuleFor(command => command.Value).NotEmpty().MaximumLength(500);
        }
    }
}
