using MediatR;

namespace Plants.Application.Commands
{
    public record DeletePostCommand(int PostId) : IRequest<DeletePostResult>;
    public record DeletePostResult(bool Deleted);
}
