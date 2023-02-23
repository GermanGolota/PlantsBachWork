using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("info")]
public class InfoController : ControllerBase
{
    private readonly IProjectionQueryService<PlantsInformation> _infoQuery;
    private readonly IProjectionQueryService<User> _userQuery;
    private readonly IIdentityProvider _identity;

    public InfoController(IProjectionQueryService<PlantsInformation> infoQuery, IProjectionQueryService<User> userQuery, IIdentityProvider identity)
    {
        _infoQuery = infoQuery;
        _userQuery = userQuery;
        _identity = identity;
    }

    public record DictsViewResult(HashSet<string> Groups, HashSet<string> Regions, HashSet<string> Soils);

    [HttpGet("dicts")]
    public async Task<ActionResult<DictsViewResult>> Dicts(CancellationToken token)
    {
        var dicts = await _infoQuery.GetByIdAsync(PlantsInformation.InfoId, token);
        return new DictsViewResult(dicts.GroupNames, dicts.RegionNames, dicts.SoilNames);
    }

    public record AddressViewResult(List<DeliveryAddress> Addresses);

    [HttpGet("addresses")]
    public async Task<ActionResult<AddressViewResult>> Addresses(CancellationToken token)
    {
        var id = _identity.Identity!.UserName.ToGuid();
        var user = await _userQuery.GetByIdAsync(id, token);
        return new AddressViewResult(user.UsedAdresses.ToList());
    }

}
