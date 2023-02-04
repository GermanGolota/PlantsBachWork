using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Plants.Presentation;

[ApiController]
[Route("file")]
public class FileController : ControllerBase
{
    private readonly IFileProvider _provider;
    private readonly IProjectionQueryService<PlantInstruction> _instructionQuery;
    private readonly IProjectionQueryService<PlantInfo> _infoQuery;

    public FileController(IFileProvider provider,
        IProjectionQueryService<PlantInstruction> instructionQuery,
        IProjectionQueryService<PlantInfo> infoQuery)
    {
        _provider = provider;
        _instructionQuery = instructionQuery;
        _infoQuery = infoQuery;
    }

    [HttpGet("instruction/{id}")]
    public async Task<ActionResult> LoadInstruction([FromRoute] Guid id, CancellationToken token)
    {
        var instruction = await _instructionQuery.GetByIdAsync(id, token);
        var path = instruction.CoverUrl;
        var info = _provider.GetFileInfo(path);
        if (info.Exists)
        {
            var streamFunc = info.CreateReadStream;
            var bytes = await streamFunc.ReadAllBytesAsync();
            return File(bytes, "application/octet-stream");
        }
        else
        {
            return NotFound();
        }
    }
}
