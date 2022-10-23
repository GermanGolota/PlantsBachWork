using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IStatsService  
{
    Task<IEnumerable<GroupFinancialStats>> GetFinancialIn(DateTime from, DateTime to);
    Task<IEnumerable<GroupTotalStats>> GetTotals();
}
