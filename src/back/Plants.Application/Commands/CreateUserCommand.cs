using MediatR;
using Plants.Shared;

namespace Plants.Application.Commands;

public record CreateUserCommand(string Login, List<UserRole> Roles, string Email, string? Language,
    string FirstName, string LastName, string PhoneNumber) : IRequest<CreateUserResult>;

public record CreateUserResult(bool Success, string Message);
