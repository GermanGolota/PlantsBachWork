using Plants.Application.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IInstructionsService
    {
        Task<IEnumerable<FindInstructionsResultItem>> GetFor(int GroupId, string? Title, string? Description);
        Task<GetInstructionResultItem?> GetBy(int Id);
    }
}
