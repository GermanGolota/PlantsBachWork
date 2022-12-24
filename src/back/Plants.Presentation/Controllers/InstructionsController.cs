using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;
using Plants.Presentation.Extensions;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("instructions")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class InstructionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstructionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("find")]
    public async Task<ActionResult<FindInstructionsResult>> Find([FromQuery] FindInstructionsRequest request)
    {
        return await _mediator.Send(request);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetInstructionResult>> Get([FromRoute] int id)
    {
        return await _mediator.Send(new GetInstructionRequest(id));
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateInstructionResult>> Create([FromForm] CreateInstructionCommandDto cmd, IFormFile file)
    {
        var bytes = file?.ReadBytes();
        var req = new CreateInstructionCommand(cmd.GroupId, cmd.Text, cmd.Title, cmd.Description, bytes);
        return await _mediator.Send(req);
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditInstructionResult>> Create(
        [FromRoute] int id, [FromForm] CreateInstructionCommandDto cmd, IFormFile file
        )
    {
        var bytes = file?.ReadBytes();
        var req = new EditInstructionCommand(id, cmd.GroupId, cmd.Text, cmd.Title, cmd.Description, bytes);
        return await _mediator.Send(req);
    }
}

public record CreateInstructionCommandDto(int GroupId, string Text,
  string Title, string Description);
