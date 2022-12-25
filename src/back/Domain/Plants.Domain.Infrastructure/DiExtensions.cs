using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Projection;
using Plants.Domain.Infrastructure.Services;
using Plants.Domain.Persistence;
using Plants.Domain.Projection;
using Plants.Domain.Services;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;
using System.Security.Cryptography;

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
        services.AddSingleton<AccessesHelper>();
        services.AddScoped<EventStoreAccessGranter>();
        services.AddTransient<RepositoryCaller>();
        services.AddTransient<EventSubscriber>();
        services.AddTransient<AggregateEventApplyer>();
        services.AddTransient(typeof(TransposeApplyer<>));
        services.AddTransient(typeof(TransposeApplyer<,>));
        services.AddScoped<CommandMetadataFactory>();
        services.AddScoped<EventMetadataFactory>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        //settings are provided through aggregates project
        services.AddScoped(factory =>
        {
            return new EventStoreClient(factory.GetRequiredService<EventStoreClientSettings>());
        });
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
        foreach (var type in Shared.Helpers.Type.Types)
        {
            if (type.IsAssignableToGenericType(baseHandlerType) && type != baseHandlerType)
            {
                foreach (var @interface in type.GetInterfaces().Where(x => x.IsAssignableToGenericType(baseHandlerType)))
                {
                    services.AddTransient(@interface, type);
                }
            }
        }
        return services;
    }

    private static IServiceCollection AddProjection(this IServiceCollection services)
    {
        services.AddScoped(factory =>
        {
            var databaseName = factory.GetRequiredService<IOptions<ConnectionConfig>>().Value.MongoDbDatabaseName;
            return factory.GetRequiredService<IMongoClientFactory>().GetDatabase(databaseName);
        });

        services.AddTransient(typeof(IProjectionRepository<>), typeof(MongoDBRepository<>));
        services.AddTransient(typeof(IProjectionQueryService<>), typeof(MongoDBRepository<>));

        var baseClassMap = new BsonClassMap(typeof(AggregateBase));
        baseClassMap.MapProperty(nameof(AggregateBase.Version));
        baseClassMap.MapProperty(nameof(AggregateBase.CommandsProcessed));
        baseClassMap.MapProperty(nameof(AggregateBase.Name));
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

        return services;
    }

 /*   private class GuidDictionarySerializer : SerializerBase<Dictionary<Guid, string>>
    {
        public GuidDictionarySerializer()
        {
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<Guid, string> values)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();
            foreach (var (key, value) in values)
            {
                writer.WriteString(key.ToString(), value);
            }
            writer.WriteEndDocument();
        }

        public override Dictionary<Guid, string> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            reader.ReadSymbol();
            reader.ReadStartDocument();
            var key = reader.ReadName();
            reader.ReadEndDocument();
        }
    }*/
}
