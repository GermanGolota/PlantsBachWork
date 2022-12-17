using EventStore.ClientAPI;
using Plants.Core;

namespace Plants.Initializer;

internal class EventStoreInitializer
{
    private readonly IEventStoreConnection _connection;

    public EventStoreInitializer(IEventStoreConnection connection)
    {
        _connection = connection;
    }

    public async Task Initialize(AccessorsDefinition definiton)
    {
        foreach (var aggregate in definiton.Flat.Select(_=>_.Aggregate).Distinct())
        {
            var meta = BuildFor(aggregate, definiton);
            await _connection.SetStreamMetadataAsync(aggregate, ExpectedVersion.Any, meta);
        }
    }


    private StreamMetadata BuildFor(string aggregateName, AccessorsDefinition definition)
    {
        var roleAccesses = definition.Defined[aggregateName];

        var managerRole = UserRole.Manager.ToString();
        var readRoles = new List<string>() { managerRole };
        var writeRoles = new List<string>() { managerRole };
        foreach (var (role, accesses) in roleAccesses)
        {
            foreach (var access in accesses)
            {
                switch (access)
                {
                    case Domain.AllowType.Read:
                        readRoles.Add(role.ToString());
                        break;
                    case Domain.AllowType.Write:
                        writeRoles.Add(role.ToString());
                        break;
                }
            }
        }

        return StreamMetadata.Build()
            .SetReadRoles(readRoles.ToArray())
            .SetWriteRoles(writeRoles.ToArray())
            .SetDeleteRole(managerRole)
            .SetMetadataReadRole(managerRole)
            .SetMetadataWriteRole(managerRole);
    }
}
