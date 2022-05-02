using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IOrderService
    {
        Task<OrderResultItem?> GetBy(int orderId);
    }
}