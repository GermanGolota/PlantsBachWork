using Plants.Shared;

namespace Plants.Domain;

public abstract class AggregateBase
{
    public AggregateBase(Guid id)
    {
        Id = id;
        Name = this.GetType().Name;
    }

    public const ulong NewAggregateVersion = 0;
    public ulong CommandsProcessed { get; private set; } = 0;
    public ulong Version { get; private set; } = NewAggregateVersion;
    public Guid Id { get; }
    public string Name { get; }

    public void Record(OneOf<Command, Event> newRecord)
    {
        Version++;
        newRecord.Match(cmd => CommandsProcessed++, _ => { });
    }
}
