using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Plants.Domain.Infrastructure.Helpers;
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
        services.AddTransient<EventSubscriber>();
        services.AddTransient<AggregateEventApplyer>();
        services.AddTransient(typeof(TransposeApplyer<>));
        services.AddTransient(typeof(TransposeApplyer<,>));
        services.AddScoped<CommandMetadataFactory>();
        services.AddScoped<EventMetadataFactory>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(_ => InfrastructureHelpers.Aggregate);
        services.AddTransient<ICommandSender, CommandSender>();
        services.AddSingleton<EventStoreConverter>();
        //works with the service scope
        services.AddScoped<IEventSubscriptionWorker, EventSubscriptionWorker>();
        services.AddTransient<IEventStore, EventStoreEventStore>();
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        return services;
    }

    private static IServiceCollection AddProjection(this IServiceCollection services)
    {
        services.AddTransient(typeof(IProjectionRepository<>), typeof(MongoDBRepository<>));
        services.AddTransient(typeof(IProjectionQueryService<>), typeof(MongoDBRepository<>));

        AddBsonConversions();

        return services;
    }

    private static void AddBsonConversions()
    {
        var baseClassMap = new BsonClassMap(typeof(AggregateBase));
        baseClassMap.MapProperty(nameof(AggregateBase.Version));
        baseClassMap.MapProperty(nameof(AggregateBase.CommandsProcessed));
        baseClassMap.MapProperty(nameof(AggregateBase.Name));
        baseClassMap.MapProperty(nameof(AggregateBase.LastUpdateTime));
        baseClassMap.MapIdProperty(nameof(AggregateBase.Id));
        var helper = InfrastructureHelpers.Aggregate;
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        foreach (var (_, type) in helper.Aggregates)
        {
            var map = new BsonClassMap(type, baseClassMap);
            var ctor = helper.AggregateCtors[type];
            map.MapConstructor(ctor, nameof(AggregateBase.Id));
            map.AutoMap();
            BsonClassMap.RegisterClassMap(map);
        }
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
