using EventStore.Client;
using Newtonsoft.Json;
using Plants.Infrastructure.Domain.Helpers;
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

    private static Command DeserializeCommand(Type eventType, ReadOnlySpan<byte> data) =>
        (Command)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, JsonSerializerSettingsContext.Settings)!;

    private static Event DeserializeEvent(Type eventType, ReadOnlySpan<byte> data) =>
        (Event)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), eventType, JsonSerializerSettingsContext.Settings)!;

}
