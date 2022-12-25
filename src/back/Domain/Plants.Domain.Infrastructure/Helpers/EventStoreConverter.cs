using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Plants.Infrastructure.Domain.Helpers;
using System.Reflection;
using System.Text;

namespace Plants.Domain.Infrastructure.Helpers;

internal class EventStoreConverter
{
    private readonly AggregateHelper _helper;

    public EventStoreConverter(AggregateHelper helper)
    {
        _helper = helper;
    }

    public OneOf<Event, Command> Convert(ResolvedEvent resolved)
    {
        OneOf<Event, Command> result;
        var eventType = resolved.Event.EventType;
        if (_helper.Events.ContainsKey(eventType))
        {
            var @event = DeserializeEvent(_helper.Events.Get(eventType), resolved.Event.Data.Span);
            result = @event;
        }
        else
        {
            if (_helper.Commands.ContainsKey(eventType))
            {
                var command = DeserializeCommand(_helper.Commands.Get(eventType), resolved.Event.Data.Span);
                result = command;
            }
            else
            {
                throw new Exception("Found unprocessable message");
            }
        }
        return result;
    }

    public byte[] Serialize(object eventOrCommand) =>
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventOrCommand));

    private static JsonSerializerSettings _settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };

    private static Command DeserializeCommand(Type eventType, ReadOnlySpan<byte> data) =>
        (Command)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, _settings)!;

    private static Event DeserializeEvent(Type eventType, ReadOnlySpan<byte> data) =>
        (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, _settings)!;

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
