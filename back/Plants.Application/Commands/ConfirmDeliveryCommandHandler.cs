using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands
{
    public class ConfirmDeliveryCommandHandler : IRequestHandler<ConfirmDeliveryCommand, ConfirmDeliveryResult>
    {
        private readonly IOrdersService _orders;

        public ConfirmDeliveryCommandHandler(IOrdersService orders)
        {
            _orders = orders;
        }

        public async Task<ConfirmDeliveryResult> Handle(ConfirmDeliveryCommand request, CancellationToken cancellationToken)
        {
            await _orders.ConfirmReceived(request.DeliveryId);
            return new ConfirmDeliveryResult(true);
        }
    }
}
