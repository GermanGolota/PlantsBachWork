using Plants.Domain;

namespace Plants.Application.Aggregates;

public class Plant : AggregateBase
{
    public Plant(Guid id) : base(id)
    {
    }
}
