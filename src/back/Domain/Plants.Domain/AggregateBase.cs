namespace Plants.Domain;

public abstract class AggregateBase
{
    public AggregateBase(Guid id)
    {
        Id = id;
        Name = this.GetType().Name;
    }

    public const long NewAggregateVersion = -1;
    public long Version { get; private set; } = NewAggregateVersion;
    public Guid Id { get; }
    public string Name { get; }

    public void BumpVersion()
    {
        Version++;
    }
}
