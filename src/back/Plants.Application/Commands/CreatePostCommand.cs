using MediatR;

namespace Plants.Application.Commands;

public record CreatePostCommand(int PlantId, decimal Price) : IRequest<CreatePostResult>;
public record CreatePostResult(bool Successfull, string Message);
