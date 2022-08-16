using MediatR;
using Plants.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class AddressRequestHandler : IRequestHandler<AddressRequest, AddressResult>
    {
        private readonly IInfoService _info;

        public AddressRequestHandler(IInfoService info)
        {
            _info = info;
        }

        public async Task<AddressResult> Handle(AddressRequest request, CancellationToken cancellationToken)
        {
            var addr = await _info.GetMyAddresses();
            return new AddressResult(addr.ToList());
        }
    }
}
