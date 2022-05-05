using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plants.Application.Commands;
using Plants.Application.Requests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Presentation.Controllers
{
    [ApiController]
    [Route("plants")]
    public class PlantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("notposted")]
        public async Task<ActionResult<PlantsResult>> GetNotPosted(CancellationToken token)
        {
            return await _mediator.Send(new PlantsRequest(), token);
        }

        [HttpGet("notposted/{id}")]
        public async Task<ActionResult<PlantResult>> GetNotPosted([FromRoute] int id, CancellationToken token)
        {
            return await _mediator.Send(new PlantRequest(id), token);
        }

        [HttpGet("prepared/{id}")]
        public async Task<ActionResult<PreparedPostResult>> GetPrepared(int id, CancellationToken token)
        {
            return await _mediator.Send(new PreparedPostRequest(id), token);
        }

        [HttpPost("{id}/post")]
        public async Task<ActionResult<CreatePostResult>> GetPrepared([FromRoute] int id,
            [FromQuery] decimal price, CancellationToken token)
        {
            return await _mediator.Send(new CreatePostCommand(id, price), token);
        }

        [HttpPost("add")]
        public async Task<ActionResult<AddPlantResult>> Create
            ([FromForm] AddPlantDto body, IEnumerable<IFormFile> files, CancellationToken token)
        {
            List<byte[]> totalBytes = ReadFiles(files);

            var request = new AddPlantCommand(body.Name, body.Description,
                body.Regions, body.SoilId, body.GroupId, body.Created, totalBytes.ToArray());

            return await _mediator.Send(request, token);
        }

        [HttpPost("{id}/edit")]
        public async Task<ActionResult<EditPlantResult>> Edit
          ([FromRoute] int id, [FromForm] EditPlantDto plant, IEnumerable<IFormFile> files, CancellationToken token)
        {
            List<byte[]> totalBytes = ReadFiles(files);

            var request = new EditPlantCommand(id, plant.PlantName, plant.PlantDescription,
                plant.RegionIds, plant.SoilId, plant.GroupId, plant.RemovedImages, totalBytes.ToArray());
            return await _mediator.Send(request, token);
        }

        private static List<byte[]> ReadFiles(IEnumerable<IFormFile> files)
        {
            var totalBytes = new List<byte[]>();
            foreach (var file in files)
            {
                using (var fileStream = file.OpenReadStream())
                {
                    byte[] bytes = new byte[file.Length];
                    fileStream.Read(bytes, 0, (int)file.Length);
                    totalBytes.Add(bytes);
                }
            }

            return totalBytes;
        }
    }

    public record AddPlantDto(string Name, string Description, int[] Regions, int SoilId, int GroupId, DateTime Created);
    public record EditPlantDto(string PlantName,
      string PlantDescription, int[] RegionIds, int SoilId, int GroupId, int[] RemovedImages);
}
