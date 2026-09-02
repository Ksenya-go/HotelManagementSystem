using FluentResults;
using HotelManagementSystem.Persistence.EfCore.Identity;
using Mediator;
using Microsoft.Extensions.Logging;

namespace HotelManagementSystem.Persistence.EfCore.Common;

public sealed class TransactionBehavior<TMessage, TResponse>(
    ApplicationDbContext dbContext,
    ILogger<TransactionBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : ICommand<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await next(message, cancellationToken);
        }

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(message, cancellationToken);

            var isSuccess = response is not ResultBase result || result.IsSuccess;

            if (isSuccess)
            {
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                logger.LogWarning(
                    "{MessageName} повернув невдалий результат — відкат транзакції",
                    typeof(TMessage).Name);

                await transaction.RollbackAsync(cancellationToken);
            }

            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}