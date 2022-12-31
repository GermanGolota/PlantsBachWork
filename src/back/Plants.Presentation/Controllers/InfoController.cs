using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("info")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class InfoController : ControllerBase
{
    private readonly IMediator _mediator;

    public InfoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dicts")]
    public async Task<ActionResult<DictsResult>> Dicts(CancellationToken token)
    {
        var req = new DictsRequest();
        var res = await _mediator.Send(req, token);
        return Ok(res);
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<AddressResult>> Addresses(CancellationToken token)
    {
        var req = new AddressRequest();
        var res = await _mediator.Send(req, token);
        return Ok(res);
    }
}

[ApiController]
[Route("v2/info")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class InfoControllerV2 : ControllerBase
{
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public InfoControllerV2(IProjectionQueryService<PlantInfo> infoQuery)
    {
        _infoQuery = infoQuery;
    }
    
    [HttpGet("dicts")]
    public async Task<ActionResult<DictsResult>> Dicts(CancellationToken token)
    {
        var dicts = await _infoQuery.GetByIdAsync(PlantInfo.InfoId);
        return new DictsResult(dicts.GroupNames, dicts.RegionNames, dicts.SoilNames);
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<AddressResult>> Addresses(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
