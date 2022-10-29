using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, DeletePostResult>
{
    private readonly IPostService _post;

    public DeletePostCommandHandler(IPostService post)
    {
        _post = post;
    }

    public Task<DeletePostResult> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        return _post.Delete(request.PostId);
    }
}
