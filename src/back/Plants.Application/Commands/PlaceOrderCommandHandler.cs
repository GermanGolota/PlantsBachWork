using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    private readonly IPostService _postService;

    public PlaceOrderCommandHandler(IPostService postService)
    {
        _postService = postService;
    }

    public Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        return _postService.Order(request.PostId, request.City, request.MailNumber);
    }
}
