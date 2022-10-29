using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Requests;

public class FindUsersRequestHandler : IRequestHandler<FindUsersRequest, FindUsersResult>
{
    private readonly IUserService _user;

    public FindUsersRequestHandler(IUserService user)
    {
        _user = user;
    }

    public async Task<FindUsersResult> Handle(FindUsersRequest request, CancellationToken cancellationToken)
    {
        var res = await _user.SearchFor(request.Name, request.Contact, request.Roles);
        return new FindUsersResult(res.ToList());
    }
}
