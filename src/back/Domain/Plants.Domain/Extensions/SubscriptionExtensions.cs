namespace Plants.Domain.Extensions;

public static class SubscriptionExtensions
{
    public static Event ChangeTargetAggregate(this Event @event, AggregateDescription description) =>
         @event with { Metadata = @event.Metadata with { Aggregate = description } };


    public static Event ChangeVersion(this Event @event, ulong version) =>
         @event with { Metadata = @event.Metadata with { EventNumber = version } };

    public static Event TransposeSubscribedEvent(this AggregateBase subscribingAggregate, Event @event) =>
         @event.ChangeTargetAggregate(subscribingAggregate.GetDescription()).ChangeVersion(subscribingAggregate.Version + 1);

    public static Command ChangeTargetAggregate(this Command command, AggregateDescription description) =>
         command with { Metadata = command.Metadata with { Aggregate = description } };
}
