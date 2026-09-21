using HotelManagementSystem.Application.Common.Behaviors;
using HotelManagementSystem.Persistence.EfCore.Common;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagementSystem.Persistence.EfCore.DependencyInjection;

public static class MediatorRegistration
{
    public static IServiceCollection AddApplicationMediator(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(TransactionBehavior<,>)
            ];
        });

        return services;
    }
}