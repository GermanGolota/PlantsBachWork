using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("instructions")]
public class InstructionsController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly IProjectionQueryService<PlantInstruction> _instructionQuery;
    private readonly ISearchQueryService<PlantInstruction, PlantInstructionParams> _instructionSearch;

    public InstructionsController(CommandHelper command,
        IProjectionQueryService<PlantInstruction> instructionQuery,
        ISearchQueryService<PlantInstruction, PlantInstructionParams> instructionSearch)
    {
        _command = command;
        _instructionQuery = instructionQuery;
        _instructionSearch = instructionSearch;
    }

    [HttpGet("find")]
    public async Task<ActionResult<FindInstructionsResult2>> Find([FromQuery] FindInstructionsRequest request, CancellationToken token)
    {
        var param = new PlantInstructionParams(request.Title, request.Description);
        var results = await _instructionSearch.SearchAsync(param, new SearchAll(), token);
        //TODO: Fix group filtering not working with elastic
        results = results.Where(_ => _.Information.GroupName == request.GroupName);
        return new FindInstructionsResult2(
            results.Select(result =>
                new FindInstructionsResultItem2(result.Id, result.Information.Title, result.Information.Description, result.CoverUrl is not null))
            .ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetInstructionResult2>> Get([FromRoute] Guid id, CancellationToken token)
    {
        GetInstructionResult2 result;
        if (await _instructionQuery.ExistsAsync(id, token))
        {
            var instruction = await _instructionQuery.GetByIdAsync(id, token);

            var information = instruction.Information;
            result = new(true, new(instruction.Id, information.Title, information.Description, information.Text, instruction.CoverUrl is not null, information.GroupName));
        }
        else
        {
            result = new(false, new());
        }
        return result;
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateInstructionResult2>> Create([FromForm] CreateInstructionCommandDto cmd, IFormFile? file, CancellationToken token)
    {
        var bytes = await file.ReadBytesAsync(token);
        var guid = new Random().GetRandomConvertableGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<CreateInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new CreateInstructionCommand(meta, new(cmd.GroupName, cmd.Text, cmd.Title, cmd.Description), bytes),
            token);
        return new CreateInstructionResult2(guid);
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditInstructionResult2>> Edit(
        [FromRoute] Guid id, [FromForm] CreateInstructionCommandDto cmd, IFormFile? file, CancellationToken token
        )
    {
        var bytes = await file.ReadBytesAsync(token);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<EditInstructionCommand>(new(id, nameof(PlantInstruction))),
            meta => new EditInstructionCommand(meta, new(cmd.GroupName, cmd.Text, cmd.Title, cmd.Description), bytes),
            token);
        return new EditInstructionResult2(id);
    }
}