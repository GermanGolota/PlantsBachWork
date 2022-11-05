using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Plants.Domain.Infrastructure.Projection;
using Plants.Domain.Persistence;
using Plants.Domain.Projection;
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
        services.AddSingleton<CQRSHelper>();
        services.AddTransient<EventStoreConnectionFactory>();
        services.AddSingleton(factory => factory.GetRequiredService<EventStoreConnectionFactory>().Create());
        services.AddSingleton<AggregateHelper>();
        services.AddTransient<ICommandSender, CommandSender>();
        services.AddTransient<IEventStore, EventStoreEventStore>();
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.RegisterExternalCommands();
        return services;
    }

    private static IServiceCollection RegisterExternalCommands(this IServiceCollection services)
    {
        var baseType = typeof(ICommandHandler<>);
        foreach (var type in Helpers.Type.Types.Where(x => x.IsAssignableToGenericType(baseType) && x != baseType))
        {
            foreach (var @interface in type.GetInterfaces().Where(x => x.IsAssignableToGenericType(baseType)))
            {
                services.AddTransient(@interface, type);
            }
        }
        return services;
    }

    private static IServiceCollection AddProjection(this IServiceCollection services)
    {
        services.AddSingleton(x => new MongoClient(x.GetRequiredService<IOptions<ConnectionConfig>>().Value.MongoDbConnection));
        services.AddSingleton(x => x.GetRequiredService<MongoClient>().GetDatabase(x.GetRequiredService<IOptions<ConnectionConfig>>().Value.MongoDbDatabaseName));

        services.AddTransient(typeof(IProjectionRepository<>), typeof(MongoDBRepository<>));
        services.AddTransient(typeof(IProjectionQueryService<>), typeof(MongoDBRepository<>));

        return services;
    }
}
