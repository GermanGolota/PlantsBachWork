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
    public ulong Version { get; private set; } = 0;
    public Guid Id { get; }
    public string Name { get; }

    public void Record(OneOf<Command, Event> newRecord)
    {
        if (CommandsProcessed != 0)
        {
            Version++;
        }
        newRecord.Match(cmd => CommandsProcessed++, _ => { });
    }
}
