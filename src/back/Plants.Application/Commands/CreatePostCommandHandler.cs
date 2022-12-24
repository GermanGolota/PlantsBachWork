using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, CreatePostResult>
{
    private readonly IPlantsService _plants;

    public CreatePostCommandHandler(IPlantsService plants)
    {
        _plants = plants;
    }

    public Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        return _plants.Post(request.PlantId, request.Price);
    }
}
