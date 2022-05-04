using Plants.Application.Commands;
using Plants.Application.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IPlantsService
    {
        Task<IEnumerable<PlantResultItem>> GetNotPosted();
        Task<PreparedPostResultItem?> GetPrepared(int plantId);
        Task<CreatePostResult> Post(int plantId, decimal price);
    }
}
