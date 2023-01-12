using MediatR;
using Microsoft.AspNetCore.Mvc;
using Plants.Aggregates.PlantInfos;
using Plants.Aggregates.PlantPosts;
using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("post")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostResult>> GetPost([FromRoute] long id)
    {
        return Ok(await _mediator.Send(new PostRequest(id)));
    }

    [HttpPost("{id}/order")]
    public async Task<ActionResult<PlaceOrderResult>> Order([FromRoute] long id, [FromQuery] string city, [FromQuery] int mailNumber)
    {
        return Ok(await _mediator.Send(new PlaceOrderCommand(id, city, mailNumber)));
    }

    [HttpPost("{id}/delete")]
    public async Task<ActionResult<DeletePostResult>> Delete([FromRoute] long id)
    {
        return await _mediator.Send(new DeletePostCommand(id));
    }
}



[ApiController]
[Route("v2/post")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class PostControllerV2 : ControllerBase
{
    private readonly IProjectionQueryService<PlantPost> _postQuery;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;
    private readonly CommandHelper _command;

    public PostControllerV2(
        IProjectionQueryService<PlantPost> postQuery,
        IProjectionQueryService<PlantInfo> infoQuery,
        CommandHelper command)
    {
        _postQuery = postQuery;
        _infoQuery = infoQuery;
        _command = command;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostResult2>> GetPost([FromRoute] Guid id, CancellationToken token)
    {
        PostResult2 result;
        if (await _postQuery.ExistsAsync(id, token))
        {
            var post = await _postQuery.GetByIdAsync(id, token);
            if (post.IsRemoved)
            {
                result = new();
            }
            else
            {
                var seller = post.Seller;
                var stock = post.Stock;
                var caretaker = stock.Caretaker;
                var plant = stock.Information;
                var images = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId, token)).PlantImagePaths.ToInverse();
                result = new(new(post.Id, plant.PlantName, plant.Description, post.Price,
                    plant.SoilName, plant.RegionNames, plant.GroupName, stock.CreatedTime,
                    seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                    caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated, 
                    stock.PictureUrls.Select(url => images[url].ToString()).ToArray()));
            }
        }
        else
        {
            result = new();
        }
        return result;
    }

    [HttpPost("{id}/order")]
    public async Task<ActionResult<PlaceOrderResult>> Order([FromRoute] Guid id, [FromQuery] string city, [FromQuery] long mailNumber, CancellationToken token = default)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<OrderPostCommand>(new(id, nameof(PlantPost))),
            meta => new OrderPostCommand(meta, new(city, mailNumber)),
            token
            );
        return result.Match<PlaceOrderResult>(
            succ => new(true, "Success"),
            fail => new(false, String.Join('\n', fail.Reasons)));
    }

    [HttpPost("{id}/delete")]
    public async Task<ActionResult<DeletePostResult>> Delete([FromRoute] Guid id, CancellationToken token = default)
    {
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<RemovePostCommand>(new(id, nameof(PlantPost))),
            meta => new RemovePostCommand(meta),
            token
            );
        return result.Match<DeletePostResult>(succ => new(true), fail => new(false));
    }
}
