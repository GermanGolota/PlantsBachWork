using MediatR;
using Plants.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class OrdersRequestHandler : IRequestHandler<OrdersRequest, OrdersResult>
    {
        private readonly IOrdersService _orders;

        public OrdersRequestHandler(IOrdersService orders)
        {
            _orders = orders;
        }

        public async Task<OrdersResult> Handle(OrdersRequest request, CancellationToken cancellationToken)
        {
            var items = await _orders.GetOrders();
            return new OrdersResult(items.ToList());
        }
    }
}
