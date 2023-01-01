using MediatR;

namespace Plants.Application.Commands;

public record StartDeliveryCommand(long OrderId, string TrackingNumber) : IRequest<StartDeliveryResult>;
public record StartDeliveryResult(bool Successfull);