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
    public async Task<ActionResult<PostResult>> GetPost([FromRoute] long id)
    {
        var guid = id.ToGuid();
        PostResult result;
        if (await _postQuery.ExistsAsync(guid))
        {
            var post = await _postQuery.GetByIdAsync(guid);
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
                var images = (await _infoQuery.GetByIdAsync(PlantInfo.InfoId)).ImagePaths.ToInverse();
                //TODO: Add sold and instructions
                result = new(new(post.Id.ToLong(), plant.PlantName, plant.Description, post.Price,
                    plant.SoilName, plant.RegionNames, plant.GroupName, stock.CreatedTime,
                    seller.FullName, seller.PhoneNumber, seller.PlantsCared, 0, 0,
                    caretaker.PlantsCared, 0, 0, stock.PictureUrls.Select(url => images[url]).ToArray()));
            }
        }
        else
        {
            result = new();
        }
        return result;
    }

    [HttpPost("{id}/order")]
    public async Task<ActionResult<PlaceOrderResult>> Order([FromRoute] long id, [FromQuery] string city, [FromQuery] long mailNumber)
    {
        var guid = id.ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<OrderPostCommand>(new(guid, nameof(PlantPost))),
            meta => new OrderPostCommand(meta, new(city, mailNumber))
            );
        return result.Match<PlaceOrderResult>(
            succ => new(true, "Success"),
            fail => new(false, String.Join('\n', fail.Reasons)));
    }

    [HttpPost("{id}/delete")]
    public async Task<ActionResult<DeletePostResult>> Delete([FromRoute] long id)
    {
        var guid = id.ToGuid();
        var result = await _command.CreateAndSendAsync(
            factory => factory.Create<RemovePostCommand>(new(guid, nameof(PlantPost))),
            meta => new RemovePostCommand(meta)
            );
        return result.Match<DeletePostResult>(succ => new(true), fail => new(false));
    }
}
