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
    public class PreparedPostRequestHandler : IRequestHandler<PreparedPostRequest, PreparedPostResult>
    {
        private readonly IPlantsService _plants;

        public PreparedPostRequestHandler(IPlantsService plants)
        {
            _plants = plants;
        }

        public async Task<PreparedPostResult> Handle(PreparedPostRequest request, CancellationToken cancellationToken)
        {
            var res = await _plants.GetPrepared(request.PlantId);
            PreparedPostResult output;
            if (res is null)
            {
                output = new PreparedPostResult();
            }
            else
            {
                output = new PreparedPostResult(res);
            }
            return output;
        }
    }
}
