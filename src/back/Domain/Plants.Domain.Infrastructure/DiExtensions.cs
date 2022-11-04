using Microsoft.Extensions.DependencyInjection;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Infrastructure.Helpers;

namespace Plants.Domain.Infrastructure;

public static class DiExtensions
{
    public static IServiceCollection AddDomainInfrastructure(this IServiceCollection services)
    {
        services.AddEventSourcing();

        return services;
    }


    private static IServiceCollection AddEventSourcing(this IServiceCollection services)
    {
        var helper = new TypeHelper();
        services.AddSingleton(helper);
        services.AddSingleton<CQRSHelper>();
        services.AddTransient<EventStoreConnectionFactory>();
        services.AddSingleton(factory => factory.GetRequiredService<EventStoreConnectionFactory>().Create());
        services.AddSingleton<AggregateHelper>();
        services.AddTransient<ICommandSender, CommandSender>();
        services.AddTransient<IEventStore, EventStoreEventStore>();
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.RegisterExternalCommands(helper);
        return services;
    }

    private static IServiceCollection RegisterExternalCommands(this IServiceCollection services, TypeHelper helper)
    {
        var baseType = typeof(ICommandHandler<>);
        foreach (var type in helper.Types.Where(x => x.IsAssignableToGenericType(baseType) && x != baseType))
        {
            foreach (var @interface in type.GetInterfaces().Where(x => x.IsAssignableToGenericType(baseType)))
            {
                services.AddTransient(@interface, type);
            }
        }
        return services;
    }

}
