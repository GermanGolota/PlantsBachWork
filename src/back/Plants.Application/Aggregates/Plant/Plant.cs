using Plants.Domain;

namespace Plants.Application.Aggregates.Plant;

public class Plant : AggregateBase
{
    public Plant(Guid id) : base(id)
    {
    }
}
