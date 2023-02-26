using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly IMediator _query;

    public OrdersController(CommandHelper command,
        IMediator query)
    {
        _command = command;
        _query = query;
    }

    [HttpGet()]
    public async Task<ActionResult<ListViewResult<OrdersViewResultItem>>> GetAll([FromQuery] bool onlyMine, CancellationToken token)
    {
        var items = await _query.Send(new SearchOrders(new(onlyMine), new QueryOptions.All()), token);
        return new ListViewResult<OrdersViewResultItem>(items);
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<CommandViewResult>> StartDelivery([FromRoute] Guid id,
        [FromQuery] string trackingNumber, CancellationToken token)
    {
        var result = await _command.SendAndWaitAsync(
            factory => factory.Create<StartOrderDeliveryCommand>(new(id, nameof(PlantOrder))),
            meta => new StartOrderDeliveryCommand(meta, trackingNumber),
            token);

        // Hack
        await Task.Delay(2000, token);

        return result.ToCommandResult();
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<CommandViewResult>> RejectOrder([FromRoute] Guid id, CancellationToken token)
    {
        var result = await _command.SendAndWaitAsync(
            factory => factory.Create<RejectOrderCommand>(new(id, nameof(PlantOrder))),
            meta => new RejectOrderCommand(meta));

        // Hack
        await Task.Delay(2000, token);

        return result.ToCommandResult();
    }

    [HttpPost("{id}/delivered")]
    public async Task<ActionResult<CommandViewResult>> MarkAsDelivered([FromRoute] Guid id, CancellationToken token)
    {
        var result = await _command.SendAndWaitAsync(
            factory => factory.Create<ConfirmDeliveryCommand>(new(id, nameof(PlantOrder))),
            meta => new ConfirmDeliveryCommand(meta),
            token);

        // Hack
        await Task.Delay(2000, token);

        return result.ToCommandResult();
    }

}
