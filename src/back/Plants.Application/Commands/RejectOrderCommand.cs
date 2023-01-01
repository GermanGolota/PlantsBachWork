using MediatR;

namespace Plants.Application.Commands;

public record RejectOrderCommand(long OrderId) : IRequest<RejectOrderResult>;
public record RejectOrderResult(bool Success);
