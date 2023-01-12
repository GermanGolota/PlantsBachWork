using EventStore.Client;
using Microsoft.Extensions.Logging;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Services;
using Plants.Infrastructure.Domain.Helpers;
using System.Text;

namespace Plants.Domain.Infrastructure;

internal class EventStoreEventStore : IEventStore
{
    private readonly IEventStoreClientFactory _clientFactory;
    private readonly AggregateHelper _helper;
    private readonly EventStoreAccessGranter _granter;
    private readonly EventStoreConverter _converter;
    private readonly ILogger<EventStoreEventStore> _logger;

    public EventStoreEventStore(IEventStoreClientFactory client, AggregateHelper helper, 
        EventStoreAccessGranter granter, EventStoreConverter converter, ILogger<EventStoreEventStore> logger)
    {
        _clientFactory = client;
        _helper = helper;
        _granter = granter;
        _converter = converter;
        _logger = logger;
    }

    public async Task<ulong> AppendEventAsync(Event @event, CancellationToken token = default)
    {
        var metadata = @event.Metadata;
        try
        {
            var eventData = new EventData(
                Uuid.FromGuid(metadata.Id),
                _helper.Events.Get(@event.GetType()),
                _converter.Serialize(@event),
                Encoding.UTF8.GetBytes("{}"));

            var writeResult = await _clientFactory.Create().AppendToStreamAsync(
                metadata.Aggregate.ToTopic(),
                metadata.EventNumber,
                new[] { eventData },
                cancellationToken: token);

            return writeResult.NextExpectedStreamRevision.ToUInt64();
        }
        catch (EventStoreException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {metadata.Id} for aggregate {metadata.Aggregate.Id}", ex);
        }
    }

    public async Task<ulong> AppendCommandAsync(Command command, ulong version, CancellationToken token = default)
    {
        var metadata = command.Metadata;
        var aggregate = metadata.Aggregate;
        try
        {
            if (version == StreamRevision.None)
            {
                await _granter.GrantAccessesAsync(aggregate, token);
            }

            var eventData = new EventData(
            Uuid.FromGuid(metadata.Id),
                _helper.Commands.Get(command.GetType()),
                _converter.Serialize(command),
                Encoding.UTF8.GetBytes("{}"));

            var writeResult = await _clientFactory.Create().AppendToStreamAsync(
                aggregate.ToTopic(),
                version,
                new[] { eventData },
                cancellationToken: token);

            return writeResult.NextExpectedStreamRevision.ToUInt64();
        }
        catch (EventStoreException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {metadata.Id} for aggregate {metadata.Aggregate.Id}", ex);
        }
    }

    public async Task<IEnumerable<CommandHandlingResult>> ReadEventsAsync(AggregateDescription aggregate, CancellationToken token = default)
    {
        try
        {
            var idToCommand = new Dictionary<Guid, Command>();
            var events = new Dictionary<Command, List<Event>>();

            var readResult = _clientFactory.Create().ReadStreamAsync(
                 Direction.Forwards,
                 aggregate.ToTopic(),
                 StreamPosition.Start,
                 cancellationToken: token
             );

            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                readResult.ReadState.Dispose();
                return Array.Empty<CommandHandlingResult>();
            }

            var readEvents = await readResult.ToListAsync(cancellationToken: token);
            readResult.ReadState.Dispose();
            foreach (var resolvedEvent in readEvents)
            {
                _converter.Convert(resolvedEvent).Match(
                    @event =>
                    {
                        var command = idToCommand[@event.Metadata.CommandId];
                        events.AddList(command, @event);
                    },
                    command =>
                    {
                        var commandId = command.Metadata.Id;
                        if (idToCommand.ContainsKey(commandId))
                        {
                            _logger.LogWarning("There is already a command with id '{commandId}' stored for '{aggregate}'", commandId, aggregate);
                        }
                        else
                        {
                            idToCommand.Add(commandId, command);
                            events[command] = new List<Event>();
                        }
                    });
            }

            return events.Select(x => new CommandHandlingResult(x.Key, x.Value.Select(_ => _))).OrderBy(_ => _.Command.Metadata.Time);
        }
        catch (EventStoreException ex)
        {
            throw new EventStoreCommunicationException($"Error while reading events for aggregate {aggregate.Id}", ex);
        }
    }
}
