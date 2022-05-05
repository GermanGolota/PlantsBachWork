using MediatR;

namespace Plants.Application.Commands
{
    public record EditPlantCommand(int PlantId, string PlantName,
        string PlantDescription, int[] RegionIds, int SoilId, int GroupId,
        int [] RemovedImages, byte[][] NewImages) : IRequest<EditPlantResult>;

    public record EditPlantResult();
}
