using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Plants.Aggregates.PlantInfos;
using Plants.Application.Contracts;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("file")]
[ApiVersion("1")]
[ApiExplorerSettings(GroupName = "v1")]
public class FileController : ControllerBase
{
    private readonly IFileService _file;

    public FileController(IFileService file)
    {
        _file = file;
    }

    [HttpGet("plant/{id}")]
    public async Task<FileResult> Load([FromRoute] long id)
    {
        var bytes = await _file.LoadPlantImage(id);
        return File(bytes, "application/octet-stream");
    }

    [HttpGet("instruction/{id}")]
    public async Task<FileResult> LoadInstruction([FromRoute] long id)
    {
        var bytes = await _file.LoadInstructionCoverImage(id);
        return File(bytes, "application/octet-stream");
    }
}

[ApiController]
[Route("v2/file")]
[ApiVersion("2")]
[ApiExplorerSettings(GroupName = "v2")]
public class FileControllerV2 : ControllerBase
{
    private readonly IFileProvider _provider;
    private readonly IProjectionQueryService<PlantInfo> _queryService;

    public FileControllerV2(IFileProvider provider, IProjectionQueryService<PlantInfo> queryService)
    {
        _provider = provider;
        _queryService = queryService;
    }

    [HttpGet("plant/{id}")]
    public async Task<ActionResult> Load([FromRoute] long id)
    {
        var plantInfo = await _queryService.GetByIdAsync(PlantInfo.InfoId);
        var path = plantInfo.PlantImagePaths[id];
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

    [HttpGet("instruction/{id}")]
    public async Task<ActionResult> LoadInstruction([FromRoute] long id)
    {
        var plantInfo = await _queryService.GetByIdAsync(PlantInfo.InfoId);
        var path = plantInfo.InstructionCoverImagePaths[id];
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
