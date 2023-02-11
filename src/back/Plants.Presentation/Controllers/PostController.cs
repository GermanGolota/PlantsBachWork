using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

[ApiController]
[Route("post")]
public class PostController : ControllerBase
{
    private readonly IProjectionQueryService<PlantPost> _postQuery;
    private readonly CommandHelper _command;

    public PostController(
        IProjectionQueryService<PlantPost> postQuery,
        CommandHelper command)
    {
        _postQuery = postQuery;
        _command = command;
    }

    public record PostViewResultItem(Guid Id, string PlantName, string Description, decimal Price,
        string[] SoilNames, string[] RegionNames, string[] GroupNames, DateTime Created,
        string SellerName, string SellerPhone, long SellerCared, long SellerSold, long SellerInstructions,
        long CareTakerCared, long CareTakerSold, long CareTakerInstructions, Picture[] Images
    )
    {
        public string CreatedHumanDate => Created.Humanize();
        public string CreatedDate => Created.ToShortDateString();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QueryViewResult<PostViewResultItem>>> GetPost([FromRoute] Guid id, CancellationToken token)
    {
        QueryViewResult<PostViewResultItem> result;
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
                result = new(new(post.Id, plant.PlantName, plant.Description, post.Price,
                    plant.SoilNames, plant.RegionNames, plant.GroupNames, stock.CreatedTime,
                    seller.FullName, seller.PhoneNumber, seller.PlantsCared, seller.PlantsSold, seller.InstructionCreated,
                    caretaker.PlantsCared, caretaker.PlantsSold, caretaker.InstructionCreated,
                    stock.Pictures));
            }
        }
        else
        {
            result = new();
        }
        return result;
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
