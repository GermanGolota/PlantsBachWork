using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IFileService
    {
        Task<byte[]> LoadPlantImage(int plantImageId);
        Task<byte[]> LoadInstructionCoverImage(int instructionId);
    }
}