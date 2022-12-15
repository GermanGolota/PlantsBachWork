using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Plants.Aggregates.Infrastructure;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Infrastructure;
using Plants.Infrastructure.Config;
using Plants.Initializer;
using Plants.Shared;
using System.Reflection;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services
            .Configure<ConnectionConfig>(ctx.Configuration.GetSection("Connection"))
            .AddShared()
            .AddDomainInfrastructure()
            .AddAggregatesInfrastructure();
            services.AddTransient<MongoDbInitializer>();
        })
        .Build();

var provider = host.Services;

var definedAccesses = Helpers.Type.Types
    .Where(x => x.IsStrictlyAssignableTo(typeof(AggregateBase)))
    .ToDictionary(
        type => type.Name,
        type => type.GetCustomAttributes<AllowAttribute>()
            .Select(attribute => (attribute.Role, attribute.Type))
            .GroupBy(x => x.Role)
            .ToDictionary(x => x.Key, x => x.Select(x => x.Type).Distinct().ToList())
    );

var flatAccesses = definedAccesses.SelectMany(pair => pair.Value.Select(pair2 => (Aggregate: pair.Key, Role: pair2.Key, Allow: pair2.Value)));
var roleToAggregates = flatAccesses.GroupBy(x => x.Role).ToDictionary(x => x.Key, x => x.Select(y => y.Aggregate).ToList());
var def = new AccessorsDefinition(definedAccesses, flatAccesses, roleToAggregates);


var mongo = provider.GetRequiredService<MongoDbInitializer>();
await mongo.CreateRoles(def);
/*
var createUserDoc = BsonDocument.Parse($$"""
    {
        "createUser":"postgres",
        "password":"password",
        "roles":[
            {
                "role": "{{UserRole.Manager}}",
                "db": "admin"
            }
        ]
    }
    """);
*/

public record AccessorsDefinition(Dictionary<string, Dictionary<UserRole, List<AllowType>>> Defined, IEnumerable<(string Aggregate, UserRole Role, List<AllowType> Allow)> Flat, Dictionary<UserRole, List<string>> RoleToAggregate);
