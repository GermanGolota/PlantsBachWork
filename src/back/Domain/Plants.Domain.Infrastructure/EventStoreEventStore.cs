using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plants.Domain.Persistence;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Shared;
using System.Reflection;
using System.Text;
using StreamPosition = EventStore.ClientAPI.StreamPosition;

namespace Plants.Domain.Infrastructure;

internal class EventStoreEventStore : IEventStore
{
    private readonly IEventStoreConnection _connection;
    private readonly AggregateHelper _helper;

    public EventStoreEventStore(IEventStoreConnection connection, AggregateHelper helper)
    {
        _connection = connection;
        _helper = helper;
    }

    public async Task<long> AppendEventAsync(Event @event)
    {
        var metadata = @event.Metadata;
        try
        {

            var eventData = new EventData(
                metadata.Id,
                _helper.Events.Get(@event.GetType()),
                true,
                Serialize(@event),
                Encoding.UTF8.GetBytes("{}"));

            var eventNumber = metadata.EventNumber - 1;
            var writeResult = await _connection.AppendToStreamAsync(
                metadata.Aggregate.Id.ToString(),
                eventNumber,
                eventData);

            return writeResult.NextExpectedVersion;
        }
        catch (EventStoreConnectionException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {metadata.Id} for aggregate {metadata.Aggregate.Id}", ex);
        }
    }

    public async Task<long> AppendCommandAsync(Command command, long version)
    {
        var metadata = command.Metadata;
        try
        {
            var eventData = new EventData(
            metadata.Id,
                _helper.Commands.Get(command.GetType()),
            true,
                Serialize(command),
                Encoding.UTF8.GetBytes("{}"));

            var eventNumber = version - 1;
            var writeResult = await _connection.AppendToStreamAsync(
                metadata.Aggregate.Id.ToString(),
                eventNumber,
                eventData);

            return writeResult.NextExpectedVersion;
        }
        catch (EventStoreConnectionException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {metadata.Id} for aggregate {metadata.Aggregate.Id}", ex);
        }
    }


    public async Task<IEnumerable<(Command Command, IEnumerable<Event> Events)>> ReadEventsAsync(Guid id)
    {
        try
        {
            var events = new List<(Command Command, List<Event> Events)>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;

            int slicesCount = 0;
            Command? lastCommand = null;
            do
            {
                slicesCount++;
                currentSlice = await _connection.ReadStreamEventsForwardAsync(id.ToString(), nextSliceStart, 200, false);
                if (currentSlice.Status != SliceReadStatus.Success)
                {
                    if (slicesCount == 1)
                    {
                        break;
                    }
                    throw new EventStoreAggregateNotFoundException($"Aggregate {id} not found");
                }
                nextSliceStart = currentSlice.NextEventNumber;
                foreach (var resolvedEvent in currentSlice.Events)
                {
                    var eventType = resolvedEvent.Event.EventType;
                    if (_helper.Events.ContainsKey(eventType))
                    {
                        var @event = DeserializeEvent(_helper.Events.Get(eventType), resolvedEvent.Event.Data);
                        if(lastCommand is null)
                        {
                            throw new Exception("First record was not a command");
                        }
                        else
                        {
                            events.Last().Events.Add(@event);
                        }
                    }
                    else
                    {
                        if (_helper.Commands.ContainsKey(eventType))
                        {
                            lastCommand = DeserializeCommand(_helper.Commands.Get(eventType), resolvedEvent.Event.Data);
                            events.Add((lastCommand, new List<Event>()));
                        }
                        else
                        {
                            throw new Exception("Found unprocessable message");
                        }
                    }
                }
            } while (currentSlice.IsEndOfStream == false);

            return events.Select(x => (x.Item1, x.Item2.Select(_ => _)));
        }
        catch (EventStoreConnectionException ex)
        {
            throw new EventStoreCommunicationException($"Error while reading events for aggregate {id}", ex);
        }
    }

    private static Command DeserializeCommand(Type eventType, byte[] data)
    {
        var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
        return (Command)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, settings);
    }

    private static Event DeserializeEvent(Type eventType, byte[] data)
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
