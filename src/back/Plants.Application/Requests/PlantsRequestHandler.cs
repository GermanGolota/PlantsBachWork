using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Requests;

public class PlantsRequestHandler : IRequestHandler<PlantsRequest, PlantsResult>
{
    private readonly IPlantsService _plants;

    public PlantsRequestHandler(IPlantsService plants)
    {
        _plants = plants;
    }

    public async Task<PlantsResult> Handle(PlantsRequest request, CancellationToken cancellationToken)
    {
        var items = await _plants.GetNotPosted();
        return new PlantsResult(items.ToList());
    }
}
