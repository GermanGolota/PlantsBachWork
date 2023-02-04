using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("info")]
public class InfoController : ControllerBase
{
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IIdentityProvider _identity;

    public InfoController(IProjectionQueryService<PlantInfo> infoQuery, IProjectionQueryService<User> userQuery, IIdentityProvider identity)
    {
        _infoQuery = infoQuery;
        _userQuery = userQuery;
        _identity = identity;
    }

    [HttpGet("dicts")]
    public async Task<ActionResult<DictsResult2>> Dicts(CancellationToken token)
    {
        var dicts = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        return new DictsResult2(dicts.GroupNames, dicts.RegionNames, dicts.SoilNames);
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<AddressResult>> Addresses(CancellationToken token)
    {
        var id = _identity.Identity!.UserName.ToGuid();
        var user = await _userQuery.GetByIdAsync(id, token);
        return new AddressResult(user.UsedAdresses.ToList());
    }

    public record AddressResult(List<DeliveryAddress> Addresses);
}
