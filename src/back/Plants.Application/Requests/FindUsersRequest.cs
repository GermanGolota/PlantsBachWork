using MediatR;
using Plants.Shared;

namespace Plants.Application.Requests;

public record FindUsersRequest(string? Name, string? Contact, UserRole[]? Roles) : IRequest<FindUsersResult>;
public record FindUsersResult(List<FindUsersResultItem> Items);
public record FindUsersResultItem(string FullName, string Mobile, string Login)
{
    //for converter
    public FindUsersResultItem() : this("", "", "")
    {

    }
    private string[] roles;

    public string[] Roles
    {
        get => roles;
        set
        {
            roles = value;
            RoleCodes = value.Select(To).ToArray();

        }
    }

    private static UserRole To(string role)
    {
        return role switch
        {
            "consumer" => UserRole.Consumer,
            "producer" => UserRole.Producer,
            "manager" => UserRole.Manager,
            _ => throw new ArgumentException("Bad role name", role)
        };
    }

    public UserRole[] RoleCodes { get; set; }
}
