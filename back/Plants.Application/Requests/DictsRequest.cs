using MediatR;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record DictsRequest() : IRequest<DictsResult>;
    public record DictsResult(Dictionary<int, string> Groups, Dictionary<int, string> Regions, Dictionary<int, string> Soils);
}
