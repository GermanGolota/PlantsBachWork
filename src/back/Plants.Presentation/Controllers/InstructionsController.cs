using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("instructions")]
public class InstructionsController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly IMediator _query;
    private readonly IPictureUploader _fileUploader;

    public InstructionsController(CommandHelper command,
        IMediator query,
        IPictureUploader fileUploader)
    {
        _command = command;
        _query = query;
        _fileUploader = fileUploader;
    }

    [HttpGet("find")]
    public async Task<ActionResult<ListViewResult<FindInstructionsViewResultItem>>> Find([FromQuery] PlantInstructionParams parameters, CancellationToken token)
    {
        var items = await _query.Send(new SearchInstructions(parameters, new QueryOptions.All()), token);
        return new ListViewResult<FindInstructionsViewResultItem>(items.ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QueryViewResult<GetInstructionViewResultItem>>> Get([FromRoute] Guid id, CancellationToken token)
    {
        var item = await _query.Send(new GetInstruction(id), token);
        return item.ToQueryResult();
    }

    public record CreateInstructionViewRequest(string GroupName, string Text, string Title, string Description);

    [HttpPost("create")]
    public async Task<ActionResult<CommandViewResult>> Create([FromForm] CreateInstructionViewRequest request, IFormFile? file, CancellationToken token)
    {
        var bytes = await file.ReadBytesAsync(token);
        var picture = await _fileUploader.UploadAsync(token, new FileView(Guid.NewGuid(), bytes));

        var guid = new Random().GetRandomConvertableGuid();
        var result = await _command.SendAndNotifyAsync(
            factory => factory.Create<CreateInstructionCommand>(new(guid, nameof(PlantInstruction))),
            meta => new CreateInstructionCommand(meta, new(request.GroupName, request.Text, request.Title, request.Description), picture.Single()),
            token);
        return result.ToCommandResult();
    }

    [HttpPost("{id}/edit")]
    public async Task<ActionResult<CommandViewResult>> Edit(
        [FromRoute] Guid id, [FromForm] CreateInstructionViewRequest cmd, IFormFile? file, CancellationToken token
        )
    {
        var bytes = await file.ReadBytesAsync(token);
        var picture = await _fileUploader.UploadAsync(token, new FileView(Guid.NewGuid(), bytes));
        
        var result = await _command.SendAndNotifyAsync(
            factory => factory.Create<EditInstructionCommand>(new(id, nameof(PlantInstruction))),
            meta => new EditInstructionCommand(meta, new(cmd.GroupName, cmd.Text, cmd.Title, cmd.Description), picture.Single()),
            token);
        return result.ToCommandResult();
    }
}