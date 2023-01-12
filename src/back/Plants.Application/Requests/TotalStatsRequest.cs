using MediatR;

namespace Plants.Application.Requests;

public record TotalStatsRequest() : IRequest<TotalStatsResult>;

public record TotalStatsResult(IEnumerable<GroupTotalStats> Groups);
public record GroupTotalStats(long GroupId, string GroupName, decimal Income, long Instructions, long Popularity);

public record TotalStatsResult2(IEnumerable<GroupTotalStats2> Groups);
public record GroupTotalStats2(string GroupId, string GroupName, decimal Income, long Instructions, long Popularity);
