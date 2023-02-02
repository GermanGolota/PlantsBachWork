using MediatR;
using Plants.Application.Contracts;
using Plants.Services;
using Plants.Shared;

namespace Plants.Application.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IUserService _user;
    private readonly IEmailer _emailer;

    public CreateUserCommandHandler(IUserService user, IEmailer emailer)
    {
        _user = user;
        _emailer = emailer;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        const int TempPasswordLength = 8;
        var tempPassword = StringHelper.GetRandomAlphanumericString(TempPasswordLength);
        var lang = request.Language ?? "English";
        await _emailer.SendInvitationEmailAsync(request.Email, request.Login, tempPassword, lang);
        return await _user.CreateUser(request.Login, request.Roles,
            request.FirstName, request.LastName, request.PhoneNumber, tempPassword);
    }
}
