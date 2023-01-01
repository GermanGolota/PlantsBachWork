using MediatR;

namespace Plants.Application.Requests;

public record AddressRequest() : IRequest<AddressResult>;
public record AddressResult(List<PersonAddress> Addresses);
public record PersonAddress(string City, long MailNumber);
