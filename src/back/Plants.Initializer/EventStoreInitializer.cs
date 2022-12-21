using EventStore.Client;
using Plants.Shared;

namespace Plants.Initializer;

internal class EventStoreInitializer
{
    private readonly EventStoreClient _client;

    public EventStoreInitializer(EventStoreClient client)
    {
        _client = client;
    }

    public async Task Initialize(AccessorsDefinition definiton)
    {
        foreach (var aggregate in definiton.Flat.Select(_ => _.Aggregate).Distinct())
        {
            var meta = BuildFor(aggregate, definiton);
            await _client.SetStreamMetadataAsync(aggregate, StreamState.Any, meta);
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

        var managerArray = new[] { managerRole };
        var acl = new StreamAcl(
            readRoles: readRoles.ToArray(), 
            writeRoles: writeRoles.ToArray(), 
            deleteRoles: managerArray, 
            metaReadRoles: managerArray, 
            metaWriteRoles: managerArray);

        return new StreamMetadata(acl: acl);
    }
}
