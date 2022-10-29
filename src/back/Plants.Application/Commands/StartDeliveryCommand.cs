using MediatR;

namespace Plants.Application.Commands;

public record ConfirmDeliveryCommand(int DeliveryId) : IRequest<ConfirmDeliveryResult>;
public record ConfirmDeliveryResult(bool Successfull);