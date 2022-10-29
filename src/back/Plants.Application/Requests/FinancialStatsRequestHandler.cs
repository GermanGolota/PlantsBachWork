using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Requests;

class FinancialStatsRequestHandler : IRequestHandler<FinancialStatsRequest, FinancialStatsResult>
{
    private readonly IStatsService _stats;

    public FinancialStatsRequestHandler(IStatsService stats)
    {
        _stats = stats;
    }

    public async Task<FinancialStatsResult> Handle(FinancialStatsRequest request, CancellationToken cancellationToken)
    {
        var results = await _stats.GetFinancialIn(request.From, request.To);
        return new FinancialStatsResult(results);
    }
}
