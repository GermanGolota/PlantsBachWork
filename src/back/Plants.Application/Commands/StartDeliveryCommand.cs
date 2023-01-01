using MediatR;

namespace Plants.Application.Commands;

public record ConfirmDeliveryCommand(long DeliveryId) : IRequest<ConfirmDeliveryResult>;
public record ConfirmDeliveryResult(bool Successfull);