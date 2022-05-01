using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IOrderService
    {
        Task<OrderResult> GetBy(int orderId);
    }
}