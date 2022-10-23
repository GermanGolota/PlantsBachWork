using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet()]
    public async Task<ActionResult<OrdersResult>> GetAll([FromQuery] bool onlyMine, CancellationToken token)
    {
        return await _mediator.Send(new OrdersRequest(onlyMine), token);
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<StartDeliveryResult>> StartDelivery([FromRoute] int id,
        [FromQuery] string trackingNumber, CancellationToken token)
    {
        return await _mediator.Send(new StartDeliveryCommand(id, trackingNumber), token);
    }

    [HttpPost("{id}/delivered")]
    public async Task<ActionResult<ConfirmDeliveryResult>> MarkAsDelivered([FromRoute] int id, CancellationToken token)
    {
        return await _mediator.Send(new ConfirmDeliveryCommand(id), token);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<RejectOrderResult>> RejectOrder([FromRoute] int id, CancellationToken token)
    {
        return await _mediator.Send(new RejectOrderCommand(id), token);
    }
}
