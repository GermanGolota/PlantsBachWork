using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class StartDeliveryCommandHandler : IRequestHandler<StartDeliveryCommand, StartDeliveryResult>
{
    private readonly IOrdersService _orders;

    public StartDeliveryCommandHandler(IOrdersService orders)
    {
        _orders = orders;
    }

    public async Task<StartDeliveryResult> Handle(StartDeliveryCommand request, CancellationToken cancellationToken)
    {
        await _orders.ConfirmStarted(request.OrderId, request.TrackingNumber);
        return new StartDeliveryResult(true);
    }
}
