using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Requests;

public class PlantRequestHandler : IRequestHandler<PlantRequest, PlantResult>
{
    private readonly IPlantsService _plants;

    public PlantRequestHandler(IPlantsService plants)
    {
        _plants = plants;
    }
    public async Task<PlantResult> Handle(PlantRequest request, CancellationToken cancellationToken)
    {
        var item = await _plants.GetBy(request.Id);
        PlantResult res;
        if (item is not null)
        {
            res = new PlantResult(item);
        }
        else
        {
            res = new PlantResult();
        }
        return res;
    }
}
