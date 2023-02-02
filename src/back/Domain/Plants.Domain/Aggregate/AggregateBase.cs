namespace Plants.Domain;

public abstract class AggregateBase
{
    public AggregateBase(Guid id)
    {
        Id = id;
        Metadata = new(GetType().Name, new());
    }

    public Guid Id { get; }

    public AggregateMetadata Metadata { get; private set; }

    public void Record(OneOf<Command, Event> newRecord)
    {
        Metadata.Record(newRecord);
    }
}

public record AggregateMetadata(string Name, List<AggregateDescription> Referenced)
{
    public ulong CommandsProcessed { get; private set; } = 0;
    public List<Guid> CommandsProcessedIds { get; set; } = new();
    public ulong Version { get; private set; } = ulong.MaxValue;
    public DateTime LastUpdateTime { get; private set; }

    public void Record(OneOf<Command, Event> newRecord)
    {
        Version++;
        newRecord.Match(cmd =>
        {
            CommandsProcessed++;
            CommandsProcessedIds.Add(cmd.Metadata.Id);
        }, _ => { });
        LastUpdateTime = newRecord.Match(cmd => cmd.Metadata.Time, @event => @event.Metadata.Time);
    }
}