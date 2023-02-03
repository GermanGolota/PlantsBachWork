using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

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

    public record OrdersResult2(List<OrdersResultItem2> Items);

    public record OrdersResultItem2(
        int Status, Guid PostId, string City,
        long MailNumber, string SellerName, string SellerContact,
        decimal Price, string? DeliveryTrackingNumber, string[] Images)
    {
        //decoder
        public OrdersResultItem2() : this(0, Guid.NewGuid(), "",
            0, "", "", 0, null, Array.Empty<string>())
        {

        }

        private DateTime ordered;
        private DateTime? deliveryStarted;
        private DateTime? shipped;

        public DateTime Ordered
        {
            get => ordered;
            set
            {
                ordered = value;
                OrderedDate = ordered.ToShortDateString();
            }
        }

        public DateTime? DeliveryStarted
        {
            get => deliveryStarted;
            set
            {
                deliveryStarted = value;
                DeliveryStartedDate = value?.ToString();
            }
        }

        public DateTime? Shipped
        {
            get => shipped;
            set
            {
                shipped = value;
                ShippedDate = value?.ToString();
            }
        }

        public string OrderedDate { get; set; }
        public string? DeliveryStartedDate { get; set; }
        public string? ShippedDate { get; set; }
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
    public record ConfirmDeliveryResult(bool Successfull);
    public record StartDeliveryResult(bool Successfull);

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

    public record RejectOrderResult(bool Success);

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
