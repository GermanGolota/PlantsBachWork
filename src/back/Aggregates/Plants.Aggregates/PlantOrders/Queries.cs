namespace Plants.Aggregates;

internal sealed class SearchOrdersHandler : IRequestHandler<SearchOrders, IEnumerable<OrdersViewResultItem>>
{
    private readonly ISearchQueryService<PlantOrder, PlantOrderParams> _query;

    public SearchOrdersHandler(ISearchQueryService<PlantOrder, PlantOrderParams> query)
    {
        _query = query;
    }

    public async Task<IEnumerable<OrdersViewResultItem>> Handle(SearchOrders request, CancellationToken token)
    {
        var items = await _query.SearchAsync(request.Parameters, request.Options, token);
        return items.Select(item =>
        {
            var seller = item.Post.Seller;
            var stock = item.Post.Stock;
            return new OrdersViewResultItem((int)item.Status, item.Post.Id,
                item.Address.City, item.Address.MailNumber, seller.FullName,
                seller.PhoneNumber, item.Post.Price, item.TrackingNumber, stock.Pictures,
                 item.OrderTime, item.DeliveryStartedTime, item.DeliveredTime);
        });
    }
}
