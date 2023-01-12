using MediatR;

namespace Plants.Application.Requests;

public record PlantsRequest() : IRequest<PlantsResult>;
public record PlantsResult(List<PlantResultItem> Items);
public record PlantResultItem(long Id, string PlantName, string Description, bool IsMine)
{
    //for decoder
    public PlantResultItem() : this(-1, "", "", false)
    {

    }
}

public record PlantsResult2(List<PlantResultItem2> Items);
public record PlantResultItem2(Guid Id, string PlantName, string Description, bool IsMine)
{
    //for decoder
    public PlantResultItem2() : this(Guid.NewGuid(), "", "", false)
    {

    }
}
