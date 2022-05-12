using Plants.Application.Commands;
using Plants.Application.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IOrdersService
    {
        Task<IEnumerable<OrdersResultItem>> GetOrders(bool onlyMine);
        Task ConfirmStarted(int orderId, string trackingNumber);
        Task ConfirmReceived(int deliveryId);
        Task<RejectOrderResult> Reject(int orderId);
    }
}
