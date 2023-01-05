using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Aggregates.Users;
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
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IIdentityProvider _identity;

    public InfoControllerV2(IProjectionQueryService<PlantInfo> infoQuery, IProjectionQueryService<User> userQuery, IIdentityProvider identity)
    {
        _infoQuery = infoQuery;
        _userQuery = userQuery;
        _identity = identity;
    }
    
    [HttpGet("dicts")]
    public async Task<ActionResult<DictsResult>> Dicts(CancellationToken token)
    {
        var dicts = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        return new DictsResult(dicts.GroupNames, dicts.RegionNames, dicts.SoilNames);
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<AddressResult>> Addresses(CancellationToken token)
    {
        var id = _identity.Identity!.UserName.ToGuid();
        var user = await _userQuery.GetByIdAsync(id, token);
        return new AddressResult(user.UsedAdresses.Select(address => new PersonAddress(address.City, address.MailNumber)).ToList());
    }
}
