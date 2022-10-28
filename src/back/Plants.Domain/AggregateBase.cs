namespace Plants.Domain;

public abstract class AggregateBase
{
    public AggregateBase(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
}
