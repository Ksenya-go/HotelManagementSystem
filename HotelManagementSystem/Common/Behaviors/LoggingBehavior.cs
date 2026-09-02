using System.Diagnostics;
using FluentResults;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HotelManagementSystem.Application.Common.Behaviors;

public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "Обробка {MessageName} розпочата",
            messageName);

        try
        {
            var response = await next(message, cancellationToken);

            stopwatch.Stop();

            if (response is ResultBase { IsFailed: true } result)
            {
                logger.LogWarning(
                    "{MessageName} завершено з помилкою за {ElapsedMs} мс: {Errors}",
                    messageName,
                    stopwatch.ElapsedMilliseconds,
                    string.Join("; ", result.Errors.Select(e => e.Message)));
            }
            else
            {
                logger.LogInformation(
                    "{MessageName} успішно оброблено за {ElapsedMs} мс",
                    messageName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{MessageName} завершився винятком за {ElapsedMs} мс",
                messageName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}