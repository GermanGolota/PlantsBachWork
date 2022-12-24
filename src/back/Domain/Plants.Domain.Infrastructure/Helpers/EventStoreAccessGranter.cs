﻿using EventStore.Client;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Shared;

namespace Plants.Domain.Infrastructure.Helpers;

internal class EventStoreAccessGranter
{
    private readonly EventStoreClient _client;
    private readonly AccessesHelper _helper;

    public EventStoreAccessGranter(EventStoreClient client, AccessesHelper helper)
    {
        _client = client;
        _helper = helper;
    }

    public async Task GrantAccessesFor(AggregateDescription aggregate)
    {
        var meta = BuildMetadataFor(aggregate.Name);
        await _client.SetStreamMetadataAsync(aggregate.ToTopic(), StreamState.NoStream, meta);
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