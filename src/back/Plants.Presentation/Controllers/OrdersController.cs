using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly ISearchQueryService<PlantOrder, PlantOrderParams> _orderQuery;

    public OrdersController(CommandHelper command,
        ISearchQueryService<PlantOrder, PlantOrderParams> orderQuery)
    {
        _command = command;
        _orderQuery = orderQuery;
    }

    public record OrdersViewResultItem(
        int Status, Guid PostId, string City,
        long MailNumber, string SellerName, string SellerContact,
        decimal Price, string? DeliveryTrackingNumber, Picture[] Images,
        DateTime Ordered, DateTime? DeliveryStarted, DateTime? Shipped)
    {
        public string OrderedDate => Ordered.ToShortDateString();
        public string? DeliveryStartedDate => DeliveryStarted?.ToShortDateString();
        public string? ShippedDate => Shipped?.ToShortDateString();
    }

    [HttpGet()]
    public async Task<ActionResult<ListViewResult<OrdersViewResultItem>>> GetAll([FromQuery] bool onlyMine, CancellationToken token)
    {
        var items = await _orderQuery.SearchAsync(new(onlyMine), new SearchAll(), token);
        return new ListViewResult<OrdersViewResultItem>(items.Select(item =>
        {
            var seller = item.Post.Seller;
            var stock = item.Post.Stock;
            return new OrdersViewResultItem((int)item.Status, item.Post.Id,
                item.Address.City, item.Address.MailNumber, seller.FullName,
                seller.PhoneNumber, item.Post.Price, item.TrackingNumber, stock.Pictures,
                 item.OrderTime, item.DeliveryStartedTime, item.DeliveredTime);
        }));
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<CommandViewResult>> StartDelivery([FromRoute] Guid id,
        [FromQuery] string trackingNumber, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<StartOrderDeliveryCommand>(new(id, nameof(PlantOrder))),
            meta => new StartOrderDeliveryCommand(meta, trackingNumber), 
            token);
        return result.ToCommandResult();
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<CommandViewResult>> RejectOrder([FromRoute] Guid id, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<RejectOrderCommand>(new(id, nameof(PlantOrder))),
            meta => new RejectOrderCommand(meta));
        return result.ToCommandResult();
    }

    [HttpPost("{id}/delivered")]
    public async Task<ActionResult<CommandViewResult>> MarkAsDelivered([FromRoute] Guid id, CancellationToken token)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<ConfirmDeliveryCommand>(new(id, nameof(PlantOrder))),
            meta => new ConfirmDeliveryCommand(meta),
            token);
        return result.ToCommandResult();
    }

}
