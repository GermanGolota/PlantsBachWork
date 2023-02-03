using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Plants.Presentation;

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



    public record PostResult2(bool Exists, PostResultItem2 Item)
    {
        public PostResult2() : this(false, null)
        {

        }

        public PostResult2(PostResultItem2 item) : this(true, item)
        {

        }
    }

    public class PostResultItem2
    {
        public Guid Id { get; set; }
        public string PlantName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string SoilName { get; set; }
        public string[] Regions { get; set; }
        public string GroupName { get; set; }
        private DateTime created;

        public DateTime Created
        {
            get { return created; }
            set
            {
                created = value;
                CreatedHumanDate = value.Humanize();
                CreatedDate = value.ToShortDateString();
            }
        }

        public string SellerName { get; set; }
        public string SellerPhone { get; set; }
        public long SellerCared { get; set; }
        public long SellerSold { get; set; }
        public long SellerInstructions { get; set; }
        public long CareTakerCared { get; set; }
        public long CareTakerSold { get; set; }
        public long CareTakerInstructions { get; set; }
        public string[] Images { get; set; }
        public PostResultItem2()
        {

        }

        public PostResultItem2(Guid id, string plantName, string description, decimal price,
            string soilName, string[] regions, string groupName, DateTime created, string sellerName,
            string sellerPhone, long sellerCared, long sellerSold, long sellerInstructions,
            long careTakerCared, long careTakerSold, long careTakerInstructions, string[] images)
        {
            Id = id;
            PlantName = plantName;
            Description = description;
            Price = price;
            SoilName = soilName;
            Regions = regions;
            GroupName = groupName;
            Created = created;
            SellerName = sellerName;
            SellerPhone = sellerPhone;
            SellerCared = sellerCared;
            SellerSold = sellerSold;
            SellerInstructions = sellerInstructions;
            CareTakerCared = careTakerCared;
            CareTakerSold = careTakerSold;
            CareTakerInstructions = careTakerInstructions;
            Images = images;
        }
        public string CreatedHumanDate { get; set; }
        public string CreatedDate { get; set; }
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
    public record PlaceOrderResult(bool Successfull, string Message);

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

    public record DeletePostResult(bool Deleted);

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
