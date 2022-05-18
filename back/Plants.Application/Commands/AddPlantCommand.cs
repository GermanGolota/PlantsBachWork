using MediatR;
using System;

namespace Plants.Application.Commands
{
    public record AddPlantCommand(string Name, string Description, int[] Regions, 
        int SoilId, int GroupId, DateTime Created, byte[][] Pictures) : IRequest<AddPlantResult>;
    public record AddPlantResult(int Id);
}
