using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantOrders;
using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("orders")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
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
    public async Task<ActionResult<StartDeliveryResult>> StartDelivery([FromRoute] long id,
        [FromQuery] string trackingNumber, CancellationToken token)
    {
        return await _mediator.Send(new StartDeliveryCommand(id, trackingNumber), token);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<RejectOrderResult>> RejectOrder([FromRoute] long id, CancellationToken token)
    {
        return await _mediator.Send(new Plants.Application.Commands.RejectOrderCommand(id), token);
    }

    [HttpPost("{id}/delivered")]
    public async Task<ActionResult<ConfirmDeliveryResult>> MarkAsDelivered([FromRoute] long id, CancellationToken token)
    {
        return await _mediator.Send(new Plants.Application.Commands.ConfirmDeliveryCommand(id), token);
    }

}


[ApiController]
[Route("v2/orders")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class OrdersControllerV2 : ControllerBase
{
    private readonly CommandHelper _command;

    public OrdersControllerV2(CommandHelper command)
    {
        _command = command;
    }

    [HttpGet()]
    public async Task<ActionResult<OrdersResult>> GetAll([FromQuery] bool onlyMine, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<StartDeliveryResult>> StartDelivery([FromRoute] long id,
        [FromQuery] string trackingNumber, CancellationToken token)
    {
        var guid = id.ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<StartOrderDeliveryCommand>(new(guid, nameof(PlantOrder))),
            meta => new StartOrderDeliveryCommand(meta, trackingNumber));
        return result.Match<StartDeliveryResult>(succ => new(true), fail => new(false));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<RejectOrderResult>> RejectOrder([FromRoute] long id, CancellationToken token)
    {
        var guid = id.ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.PlantOrders.RejectOrderCommand>(new(guid, nameof(PlantOrder))),
            meta => new Plants.Aggregates.PlantOrders.RejectOrderCommand(meta));
        return result.Match<RejectOrderResult>(succ => new(true), fail => new(false));
    }

    [HttpPost("{id}/delivered")]
    public async Task<ActionResult<ConfirmDeliveryResult>> MarkAsDelivered([FromRoute] long id, CancellationToken token)
    {
        var guid = id.ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.PlantOrders.ConfirmDeliveryCommand>(new(guid, nameof(PlantOrder))),
            meta => new Plants.Aggregates.PlantOrders.ConfirmDeliveryCommand(meta));
        return result.Match<ConfirmDeliveryResult>(succ => new(true), fail => new(false));
    }

}
