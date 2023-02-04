using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly ISearchQueryService<PlantOrder, PlantOrderParams> _orderQuery;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public OrdersController(CommandHelper command,
        ISearchQueryService<PlantOrder, PlantOrderParams> orderQuery,
        IProjectionQueryService<PlantInfo> infoQuery)
    {
        _command = command;
        _orderQuery = orderQuery;
        _infoQuery = infoQuery;
    }

    [HttpGet()]
    public async Task<ActionResult<OrdersResult2>> GetAll([FromQuery] bool onlyMine, CancellationToken token)
    {
        var items = await _orderQuery.SearchAsync(new(onlyMine), new SearchAll(), token);
        return new OrdersResult2(new(items.Select(item =>
        {
            var seller = item.Post.Seller;
            var stock = item.Post.Stock;
            return new OrdersResultItem2((int)item.Status, item.Post.Id,
                item.Address.City, item.Address.MailNumber, seller.FullName,
                seller.PhoneNumber, item.Post.Price, item.TrackingNumber, stock.Pictures)
            {
                DeliveryStarted = item.DeliveryStartedTime,
                Ordered = item.OrderTime,
                Shipped = item.DeliveredTime
            };
        }
        )));
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<StartDeliveryResult>> StartDelivery([FromRoute] Guid id,
        [FromQuery] string trackingNumber, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<StartOrderDeliveryCommand>(new(id, nameof(PlantOrder))),
            meta => new StartOrderDeliveryCommand(meta, trackingNumber), 
            token);
        return result.Match<StartDeliveryResult>(succ => new(true), fail => new(false));
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<RejectOrderResult>> RejectOrder([FromRoute] Guid id, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.RejectOrderCommand>(new(id, nameof(PlantOrder))),
            meta => new Plants.Aggregates.RejectOrderCommand(meta));
        return result.Match<RejectOrderResult>(succ => new(true), fail => new(false));
    }

    [HttpPost("{id}/delivered")]
    public async Task<ActionResult<ConfirmDeliveryResult>> MarkAsDelivered([FromRoute] Guid id, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.ConfirmDeliveryCommand>(new(id, nameof(PlantOrder))),
            meta => new Plants.Aggregates.ConfirmDeliveryCommand(meta),
            token);
        return result.Match<ConfirmDeliveryResult>(succ => new(true), fail => new(false));
    }

}
