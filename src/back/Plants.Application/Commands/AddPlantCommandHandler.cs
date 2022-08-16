using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands
{
    public class AddPlantCommandHandler : IRequestHandler<AddPlantCommand, AddPlantResult>
    {
        private readonly IPlantsService _plants;

        public AddPlantCommandHandler(IPlantsService plants)
        {
            _plants = plants;
        }

        public Task<AddPlantResult> Handle(AddPlantCommand request, CancellationToken cancellationToken)
        {
            return _plants.Create(request.Name, request.Description, request.Regions, request.SoilId, request.GroupId, request.Created, request.Pictures);
        }
    }
}
