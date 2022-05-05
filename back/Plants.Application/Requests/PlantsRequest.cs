using MediatR;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record PlantsRequest() : IRequest<PlantsResult>;
    public record PlantsResult(List<PlantResultItem> Items);
    public record PlantResultItem(int Id, string PlantName, string Description, bool IsMine)
    {
        //for decoder
        public PlantResultItem() : this(-1, "", "", false)
        {

        }
    }
}
