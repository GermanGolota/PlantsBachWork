using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Aggregates.PlantInstructions;
using Plants.Aggregates.Search;
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
    public async Task<ActionResult<GetInstructionResult>> Get([FromRoute] long id)
    {
        return await _mediator.Send(new GetInstructionRequest(id));
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateInstructionResult>> Create([FromForm] CreateInstructionCommandDto cmd, IFormFile file)
    {
        var bytes = await file?.ReadBytesAsync();
        var req = new Plants.Application.Commands.CreateInstructionCommand(cmd.GroupId, cmd.Text, cmd.Title, cmd.Description, bytes);
        return await _mediator.Send(req);
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditInstructionResult>> Edit(
        [FromRoute] int id, [FromForm] CreateInstructionCommandDto cmd, IFormFile file
        )
    {
        var bytes = await file?.ReadBytesAsync();
        var req = new Plants.Application.Commands.EditInstructionCommand(id, cmd.GroupId, cmd.Text, cmd.Title, cmd.Description, bytes);
        return await _mediator.Send(req);
    }
}

public record CreateInstructionCommandDto(long GroupId, string Text,
  string Title, string Description);

[ApiController]
[Route("v2/instructions")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class InstructionsControllerV2 : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;
    private readonly IProjectionQueryService<PlantInstruction> _instructionQuery;
    private readonly ISearchQueryService<PlantInstruction, PlantInstructionParams> _instructionSearch;

    public InstructionsControllerV2(CommandHelper command,
        IProjectionQueryService<PlantInfo> infoQuery,
        IProjectionQueryService<PlantInstruction> instructionQuery,
        ISearchQueryService<PlantInstruction, PlantInstructionParams> instructionSearch)
    {
        _command = command;
        _infoQuery = infoQuery;
        _instructionQuery = instructionQuery;
        _instructionSearch = instructionSearch;
    }

    [HttpGet("find")]
    public async Task<ActionResult<FindInstructionsResult>> Find([FromQuery] FindInstructionsRequest request)
    {
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId);
        var param = new PlantInstructionParams(info.GroupNames[request.GroupId], request.Title, request.Description);
        var results = await _instructionSearch.SearchAsync(param, new SearchAll());
        return new FindInstructionsResult(
            results.Select(result => 
                new FindInstructionsResultItem(result.Id.ToLong(), result.Information.Title, result.Information.Description, result.CoverUrl is not null))
            .ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetInstructionResult>> Get([FromRoute] long id)
    {
        var guid = id.ToGuid();
        GetInstructionResult result;
        if (await _instructionQuery.ExistsAsync(guid))
        {
            var instruction = await _instructionQuery.GetByIdAsync(guid);
            var groups = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId)).GroupNames.ToInverse();

            var information = instruction.Information;
            result = new(true, new(instruction.Id.ToLong(), information.Title, information.Description, information.Text, instruction.CoverUrl is not null, groups[information.GroupName]));
        }
        else
        {
            result = new(false, new());
        }
        return result;
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateInstructionResult>> Create([FromForm] CreateInstructionCommandDto cmd, IFormFile file)
    {
        var bytes = await file.ReadBytesAsync();
        var guid = new Random().GetRandomConvertableGuid();
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.PlantInstructions.CreateInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new Plants.Aggregates.PlantInstructions.CreateInstructionCommand(meta, new(info.GroupNames[cmd.GroupId], cmd.Text, cmd.Title, cmd.Description), bytes));
        return new CreateInstructionResult(guid.ToLong());
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditInstructionResult>> Edit(
        [FromRoute] long id, [FromForm] CreateInstructionCommandDto cmd, IFormFile file
        )
    {
        var bytes = await file.ReadBytesAsync();
        var guid = new Random().GetRandomConvertableGuid();
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.PlantInstructions.EditInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new Plants.Aggregates.PlantInstructions.EditInstructionCommand(meta, new(info.GroupNames[cmd.GroupId], cmd.Text, cmd.Title, cmd.Description), bytes));
        return new EditInstructionResult(guid.ToLong());
    }
}