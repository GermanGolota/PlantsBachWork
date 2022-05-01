using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class OrderRequestHandler : IRequestHandler<OrderRequest, OrderResult>
    {
        private readonly IOrderService _order;

        public OrderRequestHandler(IOrderService order)
        {
            _order = order;
        }

        public async Task<OrderResult> Handle(OrderRequest request, CancellationToken cancellationToken)
        {
            return await _order.GetBy(request.OrderId);
        }
    }
}
