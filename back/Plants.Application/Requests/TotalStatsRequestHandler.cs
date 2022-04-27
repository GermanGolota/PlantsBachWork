using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class TotalStatsRequestHandler : IRequestHandler<TotalStatsRequest, TotalStatsResult>
    {
        private readonly IStatsService _stats;

        public TotalStatsRequestHandler(IStatsService stats)
        {
            _stats = stats;
        }

        public async Task<TotalStatsResult> Handle(TotalStatsRequest request, CancellationToken cancellationToken)
        {
            var results = await _stats.GetTotals();
            return new TotalStatsResult(results);
        }
    }
}
