using Plants.Application.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IInfoService
    {
        Task<DictsResult> GetDicts();
        Task<IEnumerable<PersonAddress>> GetMyAddresses();
    }
}
