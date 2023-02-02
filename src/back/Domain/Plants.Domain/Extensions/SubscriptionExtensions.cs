namespace Plants.Domain;

public static class SubscriptionExtensions
{
    public static Event ChangeTargetAggregate(this Event @event, AggregateDescription description) =>
         @event with { Metadata = @event.Metadata with { Aggregate = description } };


    public static Event ChangeVersion(this Event @event, ulong version) =>
         @event with { Metadata = @event.Metadata with { EventNumber = version } };

    public static Event TransposeSubscribedEvent(this AggregateBase subscribingAggregate, Event @event) =>
         @event.ChangeTargetAggregate(subscribingAggregate.GetDescription()).ChangeVersion(subscribingAggregate.Metadata.Version + 1);

    public static Command ChangeTargetAggregate(this Command command, AggregateDescription newAggregate)
    {
        var initialAggregate = command.Metadata.InitialAggregate ?? command.Metadata.Aggregate;
        var newMeta = command.Metadata with { Aggregate = newAggregate, InitialAggregate = initialAggregate };
        return command with { Metadata = newMeta };
    }
}
