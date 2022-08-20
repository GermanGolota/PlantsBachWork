using Plants.Application.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IStatsService  
    {
        Task<IEnumerable<GroupFinancialStats>> GetFinancialIn(DateTime from, DateTime to);
        Task<IEnumerable<GroupTotalStats>> GetTotals();
    }
}
