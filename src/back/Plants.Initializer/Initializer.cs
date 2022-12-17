using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Plants.Domain;
using Plants.Shared;
using System.Reflection;

namespace Plants.Initializer;

internal class Initializer
{
    private readonly EventStoreInitializer _eventStore;
    private readonly MongoDbInitializer _mongo;
    private readonly AdminUserCreator _userCreator;
    private readonly ILogger<Initializer> _logger;

    public Initializer(EventStoreInitializer eventStore, MongoDbInitializer mongo, AdminUserCreator userCreator, ILogger<Initializer> logger)
    {
        _eventStore = eventStore;
        _mongo = mongo;
        _userCreator = userCreator;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var accesses = GetAccesses();

        await _eventStore.Initialize(accesses);
        await _mongo.Initialize(accesses);
        var result = await _userCreator.SendCreateAdminCommandAsync();
        result.Match(
            _ => _logger.LogInformation("Sucessfully initialized!"),
            fail => throw new Exception(String.Join(", ", fail.Reasons))
            );
    }

    private static AccessorsDefinition GetAccesses()
    {
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
        return new AccessorsDefinition(definedAccesses, flatAccesses, roleToAggregates);
    }
}
