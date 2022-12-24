using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Requests;

public class OrdersRequestHandler : IRequestHandler<OrdersRequest, OrdersResult>
{
    private readonly IOrdersService _orders;

    public OrdersRequestHandler(IOrdersService orders)
    {
        _orders = orders;
    }

    public async Task<OrdersResult> Handle(OrdersRequest request, CancellationToken cancellationToken)
    {
        var items = await _orders.GetOrders(request.OnlyMine);
        return new OrdersResult(items.ToList());
    }
}
