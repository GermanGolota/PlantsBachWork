using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class EditPlantCommandHandler : IRequestHandler<EditPlantCommand, EditPlantResult>
{
    private readonly IPlantsService _plants;

    public EditPlantCommandHandler(IPlantsService plants)
    {
        _plants = plants;
    }

    public async Task<EditPlantResult> Handle(EditPlantCommand request, CancellationToken cancellationToken)
    {
        await _plants.Edit(request.PlantId, request.PlantName, 
            request.PlantDescription, request.RegionIds, request.SoilId, request.GroupId,
            request.RemovedImages, request.NewImages);
        return new EditPlantResult(true, "Successfull");
    }
}
