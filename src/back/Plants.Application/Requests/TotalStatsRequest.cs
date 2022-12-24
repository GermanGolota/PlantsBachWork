using MediatR;

namespace Plants.Application.Requests;

public record TotalStatsRequest() : IRequest<TotalStatsResult>;

public record TotalStatsResult(IEnumerable<GroupTotalStats> Groups);
public record GroupTotalStats(int GroupId, string GroupName, decimal Income, long Instructions, long Popularity);
