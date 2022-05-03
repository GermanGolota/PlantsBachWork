using MediatR;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record PlantsRequest() : IRequest<PlantsResult>;
    public record PlantsResult(List<PlantResultItem> Items);
    public record PlantResultItem(int Id, string Name, string Description, bool IsMine);
}
