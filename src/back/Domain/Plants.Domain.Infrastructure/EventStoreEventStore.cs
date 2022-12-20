using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;
using System.Reflection;
using System.Text;

namespace Plants.Domain.Infrastructure;

internal class EventStoreEventStore : IEventStore
{
    private readonly EventStoreClient _client;
    private readonly AggregateHelper _helper;

    public EventStoreEventStore(EventStoreClient client, AggregateHelper helper)
    {
        _client = client;
        _helper = helper;
    }

    public async Task<ulong> AppendEventAsync(Event @event)
    {
        var metadata = @event.Metadata;
        try
        {
            var eventData = new EventData(
                Uuid.FromGuid(metadata.Id),
                _helper.Events.Get(@event.GetType()),
                Serialize(@event),
                Encoding.UTF8.GetBytes("{}"));

            var eventNumber = metadata.EventNumber - 1;
            var writeResult = await _client.AppendToStreamAsync(
                metadata.Aggregate.ToTopic(),
                eventNumber,
                new[] { eventData });

            return writeResult.NextExpectedStreamRevision.ToUInt64();
        }
        catch (EventStoreException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {metadata.Id} for aggregate {metadata.Aggregate.Id}", ex);
        }
    }

    public async Task<ulong> AppendCommandAsync(Command command, ulong version)
    {
        var metadata = command.Metadata;
        try
        {

            var eventData = new EventData(
            Uuid.FromGuid(metadata.Id),
                _helper.Commands.Get(command.GetType()),
                Serialize(command),
                Encoding.UTF8.GetBytes("{}"));

            var eventNumber = version == 0 ? StreamRevision.None : new StreamRevision(version);
            var writeResult = await _client.AppendToStreamAsync(
                metadata.Aggregate.ToTopic(),
                eventNumber,
                new[] { eventData });

            return writeResult.NextExpectedStreamRevision.ToUInt64();
        }
        catch (EventStoreException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {metadata.Id} for aggregate {metadata.Aggregate.Id}", ex);
        }
    }


    public async Task<IEnumerable<CommandHandlingResult>> ReadEventsAsync(AggregateDescription aggregate)
    {
        try
        {
            var idToCommand = new Dictionary<Guid, Command>();
            var events = new Dictionary<Command, List<Event>>();

            var readResult = _client.ReadStreamAsync(
                 Direction.Forwards,
                 aggregate.ToTopic(),
                 StreamPosition.Start
             );

            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                readResult.ReadState.Dispose();
                return Array.Empty<CommandHandlingResult>();
            }

            var readEvents = await readResult.ToListAsync();
            readResult.ReadState.Dispose();
            foreach (var resolvedEvent in readEvents)
            {
                var eventType = resolvedEvent.Event.EventType;
                if (_helper.Events.ContainsKey(eventType))
                {
                    var @event = DeserializeEvent(_helper.Events.Get(eventType), resolvedEvent.Event.Data.Span);
                    var command = idToCommand[@event.Metadata.CommandId];
                    events.AddList(command, @event);
                }
                else
                {
                    if (_helper.Commands.ContainsKey(eventType))
                    {
                        var command = DeserializeCommand(_helper.Commands.Get(eventType), resolvedEvent.Event.Data.Span);
                        idToCommand.Add(command.Metadata.Id, command);
                        events[command] = new List<Event>();
                    }
                    else
                    {
                        throw new Exception("Found unprocessable message");
                    }
                }
            }

            return events.Select(x => new CommandHandlingResult(x.Key, x.Value.Select(_ => _))).OrderBy(_ => _.Command.Metadata.Time);
        }
        catch (EventStoreException ex)
        {
            throw new EventStoreCommunicationException($"Error while reading events for aggregate {aggregate.Id}", ex);
        }
    }

    private static Command DeserializeCommand(Type eventType, ReadOnlySpan<byte> data)
    {
        var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
        return (Command)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, settings);
    }

    private static Event DeserializeEvent(Type eventType, ReadOnlySpan<byte> data)
    {
        var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
        return (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, settings);
    }

    private static byte[] Serialize(object eventOrCommand)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventOrCommand));
    }

    private class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}
