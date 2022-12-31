using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IPlantsService
{
    Task<IEnumerable<PlantResultItem>> GetNotPosted();
    Task<PreparedPostResultItem?> GetPrepared(int plantId);
    Task<CreatePostResult> Post(int plantId, decimal price);
    Task<AddPlantResult> Create(string Name, string Description,
        long[] Regions, long SoilId,
        long GroupId, DateTime Created,
        byte[][] Pictures);

    Task Edit(long PlantId, string Name, string Description,
            long[] Regions, long SoilId,
            long GroupId, long[] RemovedImages, byte[][] NewImages);
    Task<PlantResultDto?> GetBy(int id);
}
