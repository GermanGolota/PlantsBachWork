using EventStore.Client;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Infrastructure.Services;
using Plants.Shared.Model;

namespace Plants.Domain.Infrastructure.Helpers;

internal class EventStoreAccessGranter
{
    private readonly IEventStoreClientFactory _clientFactory;
    private readonly AccessesHelper _helper;

    public EventStoreAccessGranter(IEventStoreClientFactory clientFactory, AccessesHelper helper)
    {
        _clientFactory = clientFactory;
        _helper = helper;
    }

    public async Task GrantAccessesAsync(AggregateDescription aggregate, CancellationToken token = default)
    {
        var meta = BuildMetadataFor(aggregate.Name);
        await _clientFactory.Create().SetStreamMetadataAsync(aggregate.ToTopic(), StreamState.NoStream, meta, cancellationToken: token);
    }

    private StreamMetadata BuildMetadataFor(string aggregateName)
    {
        var roleAccesses = _helper.AggregateAccesses[aggregateName];

        var managerRole = UserRole.Manager.ToString();
        var readRoles = new List<string>() { managerRole };
        var writeRoles = new List<string>() { managerRole };
        foreach (var (role, accesses) in roleAccesses)
        {
            foreach (var access in accesses)
            {
                switch (access)
                {
                    case AllowType.Read:
                        readRoles.Add(role.ToString());
                        break;
                    case AllowType.Write:
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
