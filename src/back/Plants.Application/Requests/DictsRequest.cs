using MediatR;

namespace Plants.Application.Requests;

public record DictsRequest() : IRequest<DictsResult>;
public record DictsResult(Dictionary<long, string> Groups, Dictionary<long, string> Regions, Dictionary<long, string> Soils);
