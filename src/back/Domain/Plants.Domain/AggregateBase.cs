using Plants.Shared;

namespace Plants.Domain;

public abstract class AggregateBase
{
    public AggregateBase(Guid id)
    {
        Id = id;
        Name = this.GetType().Name;
    }

    public const long NewAggregateVersion = -1;
    public long CommandsProcessed { get; private set; } = 0;
    public long Version { get; private set; } = NewAggregateVersion;
    public Guid Id { get; }
    public string Name { get; }

    public void Record(OneOf<Command, Event> newRecord)
    {
        Version++;
        newRecord.Match(cmd => CommandsProcessed++, _ => { });
    }
}
