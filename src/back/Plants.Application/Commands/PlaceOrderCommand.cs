using MediatR;

namespace Plants.Application.Commands;

public record PlaceOrderCommand(long PostId, string City, int MailNumber) : IRequest<PlaceOrderResult>;
public record PlaceOrderResult(bool Successfull, string Message);
