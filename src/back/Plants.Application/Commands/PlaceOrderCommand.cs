using MediatR;

namespace Plants.Application.Commands;

public record PlaceOrderCommand(int PostId, string City, int MailNumber) : IRequest<PlaceOrderResult>;
public record PlaceOrderResult(bool Successfull, string Message);
