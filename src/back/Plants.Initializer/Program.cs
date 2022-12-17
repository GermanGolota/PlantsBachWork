using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Plants.Aggregates.Infrastructure;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Aggregates.Services;
using Plants.Aggregates.Users;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Infrastructure;
using Plants.Domain.Services;
using Plants.Infrastructure.Config;
using Plants.Initializer;
using Plants.Shared;
using System.Reflection;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services
                .Configure<ConnectionConfig>(ctx.Configuration.GetSection("Connection"))
                .Configure<AdminUserConfig>(ctx.Configuration.GetSection("Admin"))
                .AddShared()
                .AddDomainInfrastructure()
                .AddAggregatesInfrastructure();
            services.AddTransient<MongoDbInitializer>()
                    .AddTransient<EventStoreInitializer>()
                    .AddSingleton<IIdentityProvider, ConfigIdentityProvider>();
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
var eventStore = provider.GetRequiredService<EventStoreInitializer>();
var commandFactory = provider.GetRequiredService<CommandMetadataFactory>();
var adminOptions = provider.GetRequiredService<IOptions<AdminUserConfig>>().Value;
var update = provider.GetRequiredService<IUserUpdater>();
var sender = provider.GetRequiredService<ICommandSender>();
await eventStore.Initialize(def);
await mongo.Initialize(def);
var meta = commandFactory.Create<CreateUserCommand, User>(adminOptions.Username.ToGuid());
var command = new CreateUserCommand(meta,
    new UserCreationDto(
        "admin@admin.admin",
        adminOptions.Name,
        adminOptions.Name,
        "English",
        adminOptions.Username,
        "",
        Enum.GetValues<UserRole>()));
var result = await sender.SendCommandAsync(command);
