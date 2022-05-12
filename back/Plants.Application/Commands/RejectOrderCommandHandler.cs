using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands
{
    public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, RejectOrderResult>
    {
        private readonly IOrdersService _orders;

        public RejectOrderCommandHandler(IOrdersService orders)
        {
            _orders = orders;
        }

        public Task<RejectOrderResult> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
        {
            return _orders.Reject(request.OrderId);
        }
    }
}
