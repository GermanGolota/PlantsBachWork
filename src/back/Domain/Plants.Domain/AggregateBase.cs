using Plants.Shared;

namespace Plants.Domain;

public abstract class AggregateBase
{
    public AggregateBase(Guid id)
    {
        Id = id;
        Name = this.GetType().Name;
    }

    public ulong CommandsProcessed { get; private set; } = 0;
    public ulong Version { get; private set; } = UInt64.MaxValue;
    public Guid Id { get; }
    public string Name { get; }
    public DateTime LastUpdateTime { get; private set; }

    public void Record(OneOf<Command, Event> newRecord)
    {
        Version++;
        newRecord.Match(cmd => CommandsProcessed++, _ => { });
        LastUpdateTime = newRecord.Match(cmd => cmd.Metadata.Time, @event => @event.Metadata.Time);
    }
}
