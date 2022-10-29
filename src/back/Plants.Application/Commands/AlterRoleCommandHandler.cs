using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class AlterRoleCommandHandler : IRequestHandler<AlterRoleCommand, AlterRoleResult>
{
    private readonly IUserService _userService;

    public AlterRoleCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<AlterRoleResult> Handle(AlterRoleCommand request, CancellationToken cancellationToken)
    {
        switch (request.Type)
        {
            case AlterType.Remove:
                await _userService.RemoveRole(request.Login, request.Role);
                break;
            case AlterType.Add:
                await _userService.AddRole(request.Login, request.Role);
                break;
        }
        return new AlterRoleResult(true);
    }
}
