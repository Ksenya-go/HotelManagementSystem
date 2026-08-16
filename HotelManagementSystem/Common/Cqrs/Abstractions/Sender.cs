using Microsoft.Extensions.DependencyInjection;

namespace HotelManagementSystem.Application.Common.Cqrs.Abstractions;

public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public ValueTask<TResult> Send<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        return InvokeHandler<TResult>(
            typeof(ICommandHandler<,>),
            command,
            cancellationToken);
    }

    public ValueTask<TResult> Send<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        return InvokeHandler<TResult>(
            typeof(IQueryHandler<,>),
            query,
            cancellationToken);
    }

    private ValueTask<TResult> InvokeHandler<TResult>(
        Type handlerType,
        object request,
        CancellationToken cancellationToken)
    {
        var closedHandlerType = handlerType.MakeGenericType(
            request.GetType(),
            typeof(TResult));

        var handler = serviceProvider.GetRequiredService(closedHandlerType);

        var result = closedHandlerType
            .GetMethod("Handle")!
            .Invoke(handler, [request, cancellationToken]);

        return (ValueTask<TResult>)result!;
    }
}