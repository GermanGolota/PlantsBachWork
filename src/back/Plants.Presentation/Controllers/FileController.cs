using Microsoft.AspNetCore.Mvc;
using Plants.Application.Contracts;

namespace Plants.Presentation.Controllers;

[ApiController]
[Route("file")]
public class FileController : ControllerBase
{
    private readonly IFileService _file;

    public FileController(IFileService file)
    {
        _file = file;
    }

    [HttpGet("plant/{id}")]
    public async Task<FileResult> Load([FromRoute] int id)
    {
        var bytes = await _file.LoadPlantImage(id);
        return File(bytes, "application/octet-stream");
    }

    [HttpGet("instruction/{id}")]
    public async Task<FileResult> LoadInstruction([FromRoute] int id)
    {
        var bytes = await _file.LoadInstructionCoverImage(id);
        return File(bytes, "application/octet-stream");
    }
}
