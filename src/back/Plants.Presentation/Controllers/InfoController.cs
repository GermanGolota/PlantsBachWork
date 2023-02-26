using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("info")]
public class InfoController : ControllerBase
{
    private readonly IMediator _query;

    public InfoController(IMediator query)
    {
        _query = query;
    }

    [HttpGet("dicts")]
    public async Task<ActionResult<PlantSpecifications>> Dicts(CancellationToken token)
    {
        return await _query.Send(new GetUsedPlantSpecifications(), token);
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<AddressViewResult>> Addresses(CancellationToken token)
    {
        return await _query.Send(new GetOwnUsedAddresses(), token);
    }

}
