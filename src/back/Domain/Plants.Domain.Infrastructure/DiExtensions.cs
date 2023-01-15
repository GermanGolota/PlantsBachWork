using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Subscription;
using Plants.Infrastructure.Domain.Helpers;

namespace Plants.Domain.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddDomainInfrastructure(this IServiceCollection services)
    {
        services.AddEventSourcing()
            .AddProjection()
            .AddSearchProjection()
            .AddImplementations();

        return services;
    }


    private static IServiceCollection AddEventSourcing(this IServiceCollection services)
    {
        services.AddSingleton<CqrsHelper>();
        services.AddSingleton<AccessesHelper>();
        services.AddScoped<EventStoreAccessGranter>();
        services.AddTransient<RepositoriesCaller>();
        services.AddTransient<EventSubscriptionProcessor>();
        services.AddTransient<AggregateEventApplyer>();
        services.AddTransient(typeof(TransposeApplyer<>));
        services.AddTransient(typeof(TransposeApplyer<,>));
        services.AddScoped<CommandMetadataFactory>();
        services.AddScoped<EventMetadataFactory>();
        services.AddScoped<CommandHelper>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(_ => InfrastructureHelpers.Aggregate);
        services.AddTransient<ICommandSender, CommandSender>();
        services.AddSingleton<EventStoreConverter>();
        //works with the service scope
        services.AddScoped<IEventSubscription, EventSubscription>();
        services.AddTransient<AggregateEventSubscription>();
        services.AddSingleton<EventSubscriptionState>();
        services.AddSingleton<ISubscriptionProcessingNotificator, EventSubscriptionState>();
        services.AddSingleton<ISubscriptionProcessingMarker, EventSubscriptionState>();
        services.AddSingleton<IEventSubscriptionState, EventSubscriptionState>();
        services.AddTransient<IEventStore, EventStoreEventStore>();
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        return services;
    }

    private static IServiceCollection AddProjection(this IServiceCollection services)
    {
        services.AddTransient(typeof(IProjectionRepository<>), typeof(MongoDBRepository<>));
        services.AddTransient(typeof(IProjectionQueryService<>), typeof(MongoDBRepository<>));

        BsonConfigurator.Configure();

        return services;
    }

    private static IServiceCollection AddSearchProjection(this IServiceCollection services)
    {
        services.AddTransient(typeof(ISearchProjectionRepository<>), typeof(ElasticSearchProjectionRepository<>));
        services.AddTransient(typeof(ISearchQueryService<,>), typeof(ElasticSearchQueryService<,>));

        return services;
    }

    private static IServiceCollection AddImplementations(this IServiceCollection services)
    {
        foreach (var type in Shared.Helpers.Type.Types)
        {
            services
                .AddImplementationsOf(typeof(ICommandHandler<>), type)
                .AddImplementationsOf(typeof(ISearchParamsProjector<,>), type);
        }
        return services;
    }

    private static IServiceCollection AddImplementationsOf(this IServiceCollection services, Type interfaceType, Type type)
    {
        if (type.IsStrictlyAssignableToGenericType(interfaceType) && type.IsConcrete())
        {
            foreach (var @interface in type.GetInterfaces().Where(x => x.IsAssignableToGenericType(interfaceType)))
            {
                services.AddTransient(@interface, type);
            }
        }

        return services;
    }
}
