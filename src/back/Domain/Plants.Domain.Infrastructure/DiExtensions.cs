using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Projection;
using Plants.Domain.Persistence;
using Plants.Domain.Projection;
using Plants.Domain.Services;
using Plants.Infrastructure.Config;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Infrastructure.Helpers;
using Plants.Shared;

namespace Plants.Domain.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddDomainInfrastructure(this IServiceCollection services)
    {
        services.AddEventSourcing()
            .AddProjection();

        return services;
    }


    private static IServiceCollection AddEventSourcing(this IServiceCollection services)
    {
        services.AddSingleton<CqrsHelper>();
        services.AddTransient<RepositoryCaller>();
        services.AddTransient<EventSubscriber>();
        services.AddScoped<CommandMetadataFactory>();
        services.AddScoped<EventMetadataFactory>();
        services.AddTransient<EventStoreConnectionFactory>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(factory => factory.GetRequiredService<EventStoreConnectionFactory>().Create());
        services.AddSingleton(_ => InfrastructureHelpers.Aggregate);
        services.AddTransient<ICommandSender, CommandSender>();
        services.AddTransient<IEventStore, EventStoreEventStore>();
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.RegisterExternalServices();
        return services;
    }

    private static IServiceCollection RegisterExternalServices(this IServiceCollection services)
    {
        var baseHandlerType = typeof(ICommandHandler<>);
        var subType = typeof(IEventSubscriber);
        foreach (var type in Shared.Helpers.Type.Types)
        {
            if (type.IsAssignableToGenericType(baseHandlerType) && type != baseHandlerType)
            {
                foreach (var @interface in type.GetInterfaces().Where(x => x.IsAssignableToGenericType(baseHandlerType)))
                {
                    services.AddTransient(@interface, type);
                }
            }

            if (type.IsAssignableTo(subType))
            {
                services.AddTransient(type);
            }
        }
        return services;
    }

    private static IServiceCollection AddProjection(this IServiceCollection services)
    {
        services.AddSingleton(factory =>
        {
            var connectionString = factory.GetRequiredService<IOptions<ConnectionConfig>>().Value.MongoDbConnection;
            var client = new MongoClient(connectionString);
            return client;
        });
        services.AddSingleton(factory =>
        {
            var databaseName = factory.GetRequiredService<IOptions<ConnectionConfig>>().Value.MongoDbDatabaseName;
            var client = factory.GetRequiredService<MongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddTransient(typeof(IProjectionRepository<>), typeof(MongoDBRepository<>));
        services.AddTransient(typeof(IProjectionQueryService<>), typeof(MongoDBRepository<>));

        var baseClassMap = new BsonClassMap(typeof(AggregateBase));
        baseClassMap.MapProperty(nameof(AggregateBase.Version));
        baseClassMap.MapProperty(nameof(AggregateBase.Name));
        baseClassMap.MapIdProperty(nameof(AggregateBase.Id));
        var helper = InfrastructureHelpers.Aggregate;
        foreach (var (_, type) in helper.Aggregates)
        {
            var map = new BsonClassMap(type, baseClassMap);
            var ctor = helper.AggregateCtors[type];
            map.MapConstructor(ctor, nameof(AggregateBase.Id));
            map.AutoMap();
            BsonClassMap.RegisterClassMap(map);
        }

        return services;
    }
}
