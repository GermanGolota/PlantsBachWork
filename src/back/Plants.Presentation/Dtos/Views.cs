namespace Plants.Presentation;

// stats

public record FinancialStatsResult2(IEnumerable<GroupFinancialStats2> Groups);
public class GroupFinancialStats2
{
    public decimal Income { get; set; }
    public string GroupName { get; set; }
    public long SoldCount { get; set; }
    public double PercentSold { get; set; }
}

public record TotalStatsResult2(IEnumerable<GroupTotalStats2> Groups);
public record GroupTotalStats2(string GroupName, decimal Income, long Instructions, long Popularity);

// user

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

public record AlterRoleResult(bool Successfull);

public record CreateUserResult(bool Success, string Message);

public record CreateUserCommandView(string Login, List<UserRole> Roles, string Email, string? Language,
    string FirstName, string LastName, string PhoneNumber);

public record ChangePasswordResult(bool Success, string Message)
{
    public ChangePasswordResult() : this(true, "")
    {

    }

    public ChangePasswordResult(string msg) : this(false, msg)
    {

    }
}

public record PasswordChangeDto(string Password);

