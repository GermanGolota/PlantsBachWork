using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequest command, CancellationToken token)
        {
            var res = await _mediator.Send(command, token);
            ActionResult result;
            if (res.IsSuccessfull)
            {
                result = Ok(res);
            }
            else
            {
                result = Unauthorized();
            }
            return result;
        }
    }
}
