using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("post")]
public class PostController : ControllerBase
{
    private readonly CommandHelper _command;
    private readonly IMediator _query;

    public PostController(
        CommandHelper command,
        IMediator query)
    {
        _command = command;
        _query = query;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QueryViewResult<PostViewResultItem>>> GetPost([FromRoute] Guid id, CancellationToken token)
    {
        var item = await _query.Send(new GetPost(id), token);
        return item.ToQueryResult();
    }

    [HttpPost("{id}/order")]
    public async Task<ActionResult<CommandViewResult>> Order([FromRoute] Guid id, [FromQuery] string city, [FromQuery] long mailNumber, CancellationToken token = default)
    {
        var result = await _command.SendAndNotifyAsync(
            factory => factory.Create<OrderPostCommand>(new(id, nameof(PlantPost))),
            meta => new OrderPostCommand(meta, new(city, mailNumber)),
            token
            );
        return result.ToCommandResult();
    }

    [HttpPost("{id}/delete")]
    public async Task<ActionResult<CommandViewResult>> Delete([FromRoute] Guid id, CancellationToken token = default)
    {
        var result = await _command.SendAndWaitAsync(
            factory => factory.Create<RemovePostCommand>(new(id, nameof(PlantPost))),
            meta => new RemovePostCommand(meta),
            token
            );
        return result.ToCommandResult();
    }
}
