using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IInfoService
    {
        Task<DictsResult> GetDicts();
    }
}
