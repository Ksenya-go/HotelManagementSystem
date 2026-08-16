namespace HotelManagementSystem.Application.Common.Cqrs.Abstractions;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    ValueTask<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}

