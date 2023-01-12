using MediatR;

namespace Plants.Application.Commands;

public record AddPlantCommand(string Name, string Description, long[] Regions,
    long SoilId, long GroupId, DateTime Created, byte[][] Pictures) : IRequest<AddPlantResult>;
public record AddPlantResult(long Id);

public record AddPlantResult2(Guid Id);
