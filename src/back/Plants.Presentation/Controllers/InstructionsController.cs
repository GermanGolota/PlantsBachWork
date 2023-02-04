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


    public record FindInstructionsViewRequest(string GroupName, string? Title, string? Description);
    public record FindInstructionsViewResultItem(Guid Id, string Title, string Description, bool HasCover);

    [HttpGet("find")]
    public async Task<ActionResult<ListViewResult<FindInstructionsViewResultItem>>> Find([FromQuery] FindInstructionsViewRequest request, CancellationToken token)
    {
        var param = new PlantInstructionParams(request.Title, request.Description);
        var results = await _instructionSearch.SearchAsync(param, new SearchAll(), token);
        //TODO: Fix group filtering not working with elastic
        results = results.Where(_ => _.Information.GroupName == request.GroupName);
        return new ListViewResult<FindInstructionsViewResultItem>(
            results.Select(result =>
                new FindInstructionsViewResultItem(result.Id, result.Information.Title, result.Information.Description, result.CoverUrl is not null))
            );
    }

    public record GetInstructionViewResultItem(Guid Id, string Title, string Description,
        string InstructionText, bool HasCover, string PlantGroupName);

    [HttpGet("{id}")]
    public async Task<ActionResult<QueryViewResult<GetInstructionViewResultItem>>> Get([FromRoute] Guid id, CancellationToken token)
    {
        QueryViewResult<GetInstructionViewResultItem> result;
        if (await _instructionQuery.ExistsAsync(id, token))
        {
            var instruction = await _instructionQuery.GetByIdAsync(id, token);

            var information = instruction.Information;
            result = new(new(instruction.Id, information.Title, information.Description, information.Text, instruction.CoverUrl is not null, information.GroupName));
        }
        else
        {
            result = new();
        }
        return result;
    }

    public record CreateInstructionViewRequest(string GroupName, string Text, string Title, string Description);

    [HttpPost("create")]
    public async Task<ActionResult<Guid>> Create([FromForm] CreateInstructionViewRequest request, IFormFile? file, CancellationToken token)
    {
        var bytes = await file.ReadBytesAsync(token);
        var guid = new Random().GetRandomConvertableGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<CreateInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new CreateInstructionCommand(meta, new(request.GroupName, request.Text, request.Title, request.Description), bytes),
            token);
        return guid;
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<Guid>> Edit(
        [FromRoute] Guid id, [FromForm] CreateInstructionViewRequest cmd, IFormFile? file, CancellationToken token
        )
    {
        var bytes = await file.ReadBytesAsync(token);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<EditInstructionCommand>(new(id, nameof(PlantInstruction))),
            meta => new EditInstructionCommand(meta, new(cmd.GroupName, cmd.Text, cmd.Title, cmd.Description), bytes),
            token);
        return id;
    }
}