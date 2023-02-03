using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Plants.Presentation.InstructionsControllerV2;

namespace Plants.Presentation;

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

    public record FindInstructionsResult2(List<FindInstructionsResultItem2> Items);
    public record FindInstructionsResultItem2(Guid Id, string Title, string Description, bool HasCover)
    {
        public FindInstructionsResultItem2() : this(Guid.NewGuid(), "", "", false)
        {
        }
    }

    public record FindInstructionsRequest(long GroupId, string? Title, string? Description);

    [HttpGet("find")]
    public async Task<ActionResult<FindInstructionsResult2>> Find([FromQuery] FindInstructionsRequest request, CancellationToken token)
    {
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        var groupName = info.GroupNames[request.GroupId];
        var param = new PlantInstructionParams(request.Title, request.Description);
        var results = await _instructionSearch.SearchAsync(param, new SearchAll(), token);
        //TODO: Fix group filtering not working with elastic
        results = results.Where(_ => _.Information.GroupName == groupName);
        return new FindInstructionsResult2(
            results.Select(result =>
                new FindInstructionsResultItem2(result.Id, result.Information.Title, result.Information.Description, result.CoverUrl is not null))
            .ToList());
    }

    public record GetInstructionResult2(bool Exists, GetInstructionResultItem2 Item);
    public record GetInstructionResultItem2(Guid Id, string Title, string Description,
        string InstructionText, bool HasCover, string PlantGroupId)
    {
        //decoder
        public GetInstructionResultItem2() : this(Guid.NewGuid(), "", "", "", false, "-1")
        {

        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetInstructionResult2>> Get([FromRoute] Guid id, CancellationToken token)
    {
        GetInstructionResult2 result;
        if (await _instructionQuery.ExistsAsync(id, token))
        {
            var instruction = await _instructionQuery.GetByIdAsync(id, token);
            var groups = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token)).GroupNames.ToInverse();

            var information = instruction.Information;
            result = new(true, new(instruction.Id, information.Title, information.Description, information.Text, instruction.CoverUrl is not null, groups[information.GroupName].ToString()));
        }
        else
        {
            result = new(false, new());
        }
        return result;
    }

    public record CreateInstructionResult2(Guid Id);

    [HttpPost("create")]
    public async Task<ActionResult<CreateInstructionResult2>> Create([FromForm] CreateInstructionCommandDto cmd, IFormFile? file, CancellationToken token)
    {
        var bytes = await file.ReadBytesAsync(token);
        var guid = new Random().GetRandomConvertableGuid();
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.CreateInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new Plants.Aggregates.CreateInstructionCommand(meta, new(info.GroupNames[cmd.GroupId], cmd.Text, cmd.Title, cmd.Description), bytes),
            token);
        return new CreateInstructionResult2(guid);
    }

    public record EditInstructionResult2(Guid InstructionId);

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<EditInstructionResult2>> Edit(
        [FromRoute] long id, [FromForm] CreateInstructionCommandDto cmd, IFormFile? file, CancellationToken token
        )
    {
        var bytes = await file.ReadBytesAsync(token);
        var guid = new Random().GetRandomConvertableGuid();
        var info = await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token);
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<Plants.Aggregates.EditInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new Plants.Aggregates.EditInstructionCommand(meta, new(info.GroupNames[cmd.GroupId], cmd.Text, cmd.Title, cmd.Description), bytes),
            token);
        return new EditInstructionResult2(guid);
    }
}