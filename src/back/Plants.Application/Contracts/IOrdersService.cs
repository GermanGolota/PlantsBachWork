using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IOrdersService
{
    Task<IEnumerable<OrdersResultItem>> GetOrders(bool onlyMine);
    Task ConfirmStarted(long orderId, string trackingNumber);
    Task ConfirmReceived(long deliveryId);
    Task<RejectOrderResult> Reject(long orderId);
}
