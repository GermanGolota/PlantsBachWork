using MediatR;

namespace Plants.Application.Requests;

public record DictsRequest() : IRequest<DictsResult>;
public record DictsResult(Dictionary<long, string> Groups, Dictionary<long, string> Regions, Dictionary<long, string> Soils);
public record DictsResult2(Dictionary<string, string> Groups, Dictionary<string, string> Regions, Dictionary<string, string> Soils);
