using MediatR;

namespace Plants.Application.Commands
{
    public record RejectOrderCommand(int OrderId) : IRequest<RejectOrderResult>;
    public record RejectOrderResult(bool Success);
}
