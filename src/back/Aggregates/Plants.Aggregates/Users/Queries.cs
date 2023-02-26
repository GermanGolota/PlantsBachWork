using System.Data;

namespace Plants.Aggregates;

internal sealed class SearchUsersHandler : IRequestHandler<SearchUsers, IEnumerable<FindUsersResultItem>>
{
    private readonly ISearchQueryService<User, UserSearchParams> _search;

    public SearchUsersHandler(ISearchQueryService<User, UserSearchParams> search)
    {
        _search = search;
    }

    public async Task<IEnumerable<FindUsersResultItem>> Handle(SearchUsers request, CancellationToken token)
    {
        var results = await _search.SearchAsync(request.Parameters, request.Options, token);
        return results.Select(user => new FindUsersResultItem(user.Id, user.FullName, user.PhoneNumber, user.Login, user.Roles));
    }
}

internal sealed class GetOwnUsedAddressesHandler : IRequestHandler<GetOwnUsedAddresses, AddressViewResult>
{
    private readonly IProjectionQueryService<User> _query;
    private readonly IIdentityProvider _identityProvider;

    public GetOwnUsedAddressesHandler(IProjectionQueryService<User> query, IIdentityProvider identityProvider)
    {
        _query = query;
        _identityProvider = identityProvider;
    }

    public async Task<AddressViewResult> Handle(GetOwnUsedAddresses request, CancellationToken cancellationToken)
    {
        var id = _identityProvider.Identity!.UserName.ToGuid();
        var user = await _query.GetByIdAsync(id, cancellationToken);
        return new AddressViewResult(user.UsedAdresses.ToList());
    }
}
