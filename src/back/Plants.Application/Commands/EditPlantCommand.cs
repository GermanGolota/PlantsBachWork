using MediatR;

namespace Plants.Application.Commands;

public record EditPlantCommand(long PlantId, string PlantName,
    string PlantDescription, long[] RegionIds, long SoilId, long GroupId,
    long[] RemovedImages, byte[][] NewImages) : IRequest<EditPlantResult>;

public record EditPlantResult(bool Success, string Message);
