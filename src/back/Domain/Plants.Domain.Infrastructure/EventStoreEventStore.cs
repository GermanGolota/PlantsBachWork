﻿using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
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

    public async Task<IEnumerable<Event>> ReadEventsAsync(Guid id)
    {
        try
        {
            var events = new List<Event>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;

            int slicesCount = 0;

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
                    events.Add(Deserialize(_helper.Events.Get(resolvedEvent.Event.EventType), resolvedEvent.Event.Data));
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

    private static Event Deserialize(Type eventType, byte[] data)
    {
        var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
        return (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, settings);
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
