using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("stats")]
    public class StatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("financial")]
        public async Task<ActionResult<FinancialStatsResult>> Financial([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken token)
        {
            var req = new FinancialStatsRequest(from, to);
            var res = await _mediator.Send(req, token);
            return Ok(res);
        }

        [HttpGet("total")]
        public async Task<ActionResult<TotalStatsResult>> Total(CancellationToken token)
        {
            var req = new TotalStatsRequest();
            var res = await _mediator.Send(req, token);
            return Ok(res);
        }
    }
}
