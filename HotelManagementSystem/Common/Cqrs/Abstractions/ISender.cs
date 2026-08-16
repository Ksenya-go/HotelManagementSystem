namespace HotelManagementSystem.Application.Common.Cqrs.Abstractions;

public interface ISender
{
    ValueTask<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken 
        cancellationToken = default);
    ValueTask<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken 
        cancellationToken = default);
}

