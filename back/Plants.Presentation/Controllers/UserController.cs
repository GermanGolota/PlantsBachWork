using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;
using Plants.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<ActionResult<FindUsersResult>> Search(
            [FromQuery] string? name, [FromQuery] string? phone, [FromQuery] UserRole[]? roles)
        {
            return await _mediator.Send(new FindUsersRequest(name, phone, roles));
        }
    }
}
