using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Commands;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly IUserService _user;

    public ChangePasswordCommandHandler(IUserService user)
    {
        _user = user;
    }

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        List<Func<string, ChangePasswordResult>> validations = new List<Func<string, ChangePasswordResult>>()
        {
            SustainableLength,
            HasDigits,
            HasLetters
        };

        var failures = validations
            .Select(validation => validation(request.NewPassword))
            .Where(x => x.Success == false);

        ChangePasswordResult finalResult;
        if (failures.Any())
        {
            string msg = String.Join('\n', failures.Select(x => x.Message));
            finalResult = new ChangePasswordResult(false, msg);
        }
        else
        {
            await _user.ChangeMyPassword(request.NewPassword);
            finalResult = new ChangePasswordResult(true, "Successfully updated");
        }
        return finalResult;
    }

    private static ChangePasswordResult SustainableLength(string password)
    {
        const int MIN_LENGTH = 5;
        ChangePasswordResult res;
        if (password.Length < MIN_LENGTH)
        {
            res = new ChangePasswordResult($"Length should be greater than {MIN_LENGTH} characters!");
        }
        else
        {
            res = new ChangePasswordResult();
        }
        return res;
    }

    private static ChangePasswordResult HasDigits(string password)
    {
        ChangePasswordResult res;
        if (password.Any(x => Char.IsDigit(x)))
        {
            res = new ChangePasswordResult();
        }
        else
        {
            res = new ChangePasswordResult("Password must contain digits");
        }
        return res;
    }

    private static ChangePasswordResult HasLetters(string password)
    {
        ChangePasswordResult res;
        if (password.Any(x => Char.IsLetter(x)))
        {
            res = new ChangePasswordResult();
        }
        else
        {
            res = new ChangePasswordResult("Password must contain digits");
        }
        return res;
    }
}
