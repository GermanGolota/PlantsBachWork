using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plants.Application.Requests
{
    public record FinancialStatsRequest(DateTime From, DateTime To) : IRequest<FinancialStatsResult>;

    public record FinancialStatsResult(IEnumerable<GroupFinancialStats> Groups);
    public class GroupFinancialStats
    {
        public decimal Income { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public long SoldCount { get; set; }
        public double PercentSold { get; set; }
    }
}
