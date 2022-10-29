using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plants.Domain;
using Plants.Domain.Persistence;
using System.Reflection;
using System.Text;
using StreamPosition = EventStore.ClientAPI.StreamPosition;

namespace Plants.Infrastructure.Domain;

public class EventStoreEventStore : IEventStore
{
    private readonly IEventStoreConnection _connection;

    public EventStoreEventStore(IEventStoreConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<Event>> ReadEventsAsync(Guid id)
    {
        try
        {
            var events = new List<Event>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;

            do
            {
                currentSlice = await _connection.ReadStreamEventsForwardAsync(id.ToString(), nextSliceStart, 200, false);
                if (currentSlice.Status != SliceReadStatus.Success)
                {
                    throw new EventStoreAggregateNotFoundException($"Aggregate {id} not found");
                }
                nextSliceStart = currentSlice.NextEventNumber;
                foreach (var resolvedEvent in currentSlice.Events)
                {
                    events.Add(Deserialize(resolvedEvent.Event.EventType, resolvedEvent.Event.Data));
                }
            } while (currentSlice.IsEndOfStream == false);

            return events;
        }
        catch (EventStoreConnectionException ex)
        {
            throw new EventStoreCommunicationException($"Error while reading events for aggregate {id}", ex);
        }
    }

    public async Task<long> AppendEventAsync(Event @event)
    {
        try
        {
            var eventData = new EventData(
                @event.Id,
                @event.GetType().AssemblyQualifiedName,
                true,
                Serialize(@event),
                Encoding.UTF8.GetBytes("{}"));

            var writeResult = await _connection.AppendToStreamAsync(
                @event.Aggregate.Id.ToString(),
                @event.EventNumber == AggregateBase.NewAggregateVersion ? ExpectedVersion.NoStream : @event.EventNumber,
                eventData);

            return writeResult.NextExpectedVersion;
        }
        catch (EventStoreConnectionException ex)
        {
            throw new EventStoreCommunicationException($"Error while appending event {@event.Id} for aggregate {@event.Aggregate.Id}", ex);
        }
    }

    private static Event Deserialize(string eventType, byte[] data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
        return (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType(eventType), settings);
    }

    private static byte[] Serialize(Event @event)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
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
