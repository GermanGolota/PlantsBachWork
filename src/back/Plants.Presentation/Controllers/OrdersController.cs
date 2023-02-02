using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Presentation;

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
    private readonly ISearchQueryService<PlantOrder, PlantOrderParams> _orderQuery;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public OrdersControllerV2(CommandHelper command,
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
        var images = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token)).PlantImagePaths.ToInverse();

        var items = await _orderQuery.SearchAsync(new(onlyMine), new SearchAll(), token);
        return new OrdersResult2(new(items.Select(item =>
        {
            var seller = item.Post.Seller;
            var stock = item.Post.Stock;
            return new OrdersResultItem2((int)item.Status, item.Post.Id,
                item.Address.City, item.Address.MailNumber, seller.FullName,
                seller.PhoneNumber, item.Post.Price, item.TrackingNumber, stock.PictureUrls.Select(url => images[url].ToString()).ToArray())
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
