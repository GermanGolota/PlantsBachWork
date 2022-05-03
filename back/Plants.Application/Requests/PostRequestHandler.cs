using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class PostRequestHandler : IRequestHandler<PostRequest, PostResult>
    {
        private readonly IPostService _post;

        public PostRequestHandler(IPostService post)
        {
            _post = post;
        }

        public async Task<PostResult> Handle(PostRequest request, CancellationToken cancellationToken)
        {
            var item = await _post.GetBy(request.PostId);
            PostResult res;
            if (item is not null)
            {
                res = new PostResult(item);
            }
            else
            {
                res = new PostResult();
            }
            return res;
        }
    }
}
