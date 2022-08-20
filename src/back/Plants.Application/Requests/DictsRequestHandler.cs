using MediatR;
using Plants.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class DictsRequestHandler : IRequestHandler<DictsRequest, DictsResult>
    {
        private readonly IInfoService _service;

        public DictsRequestHandler(IInfoService service)
        {
            _service = service;
        }

        public Task<DictsResult> Handle(DictsRequest request, CancellationToken cancellationToken)
        {
            return _service.GetDicts();
        }
    }
}
