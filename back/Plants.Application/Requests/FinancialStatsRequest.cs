using MediatR;
using System;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record FinancialStatsRequest(DateTime From, DateTime To) : IRequest<FinancialStatsResult>;

    public record FinancialStatsResult(IEnumerable<GroupFinancialStats> Groups);
    public record GroupFinancialStats(int GroupId, string GroupName, long SoldCount, double PercentSold, decimal Income);
}
