using MediatR;

namespace Plants.Application.Commands
{
    public record ChangePasswordCommand(string NewPassword) : IRequest<ChangePasswordResult>;
    public record ChangePasswordResult(bool Success, string Message)
    {
        public ChangePasswordResult() : this(true, "")
        {

        }

        public ChangePasswordResult(string msg) : this(false, msg)
        {

        }
    }
}
