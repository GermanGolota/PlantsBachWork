using Plants.Shared.Model;

namespace Plants.Application.Contracts;

public record CredsResponse(bool IsValid, UserRole[]? Roles)
{
    public CredsResponse() : this(false, null) { }
    public CredsResponse(UserRole[] roles) : this(true, roles) { }
}
