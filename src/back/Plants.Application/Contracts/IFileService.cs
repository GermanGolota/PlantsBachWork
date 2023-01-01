namespace Plants.Application.Contracts;

public interface IFileService
{
    Task<byte[]> LoadPlantImage(long plantImageId);
    Task<byte[]> LoadInstructionCoverImage(long instructionId);
}