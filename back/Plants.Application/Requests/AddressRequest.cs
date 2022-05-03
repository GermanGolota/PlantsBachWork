﻿using MediatR;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record AddressRequest() : IRequest<AddressResult>;
    public record AddressResult(List<PersonAddress> Addresses);
    public record PersonAddress(string City, int MailNumber);
}
