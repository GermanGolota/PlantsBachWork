using Plants.Application.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IPlantsService
    {
        Task<IEnumerable<PlantResultItem>> GetNotPosted();
    }
}
