namespace Plants.Aggregates.PlantOrders;

internal class ConfirmDeliveryCommandHandler : ICommandHandler<ConfirmDeliveryCommand>
{
    private readonly IQueryService<PlantOrder> _repo;
    private PlantOrder _order;

    public ConfirmDeliveryCommandHandler(IQueryService<PlantOrder> repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Event>> HandleAsync(ConfirmDeliveryCommand command, CancellationToken token = default) =>
        (new[]
        {
            new DeliveryConfirmedEvent(EventFactory.Shared.Create<DeliveryConfirmedEvent>(command), _order.Post.Seller.Login, _order.Post.Stock.Information.GroupName, _order.Post.Price)
        }).ToResultTask<IEnumerable<Event>>();

    public async Task<CommandForbidden?> ShouldForbidAsync(ConfirmDeliveryCommand command, IUserIdentity user, CancellationToken token = default)
    {
        _order ??= await _repo.GetByIdAsync(command.Metadata.Aggregate.Id, token: token);
        return user.HasRole(Manager).Or(user.HasRole(Producer).And(_order.IsBuyer(user)))
            .And(_order.StatusIs(OrderStatus.Delivering));
    }


}
