using Plants.Application.Commands;
using Plants.Application.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IPlantsService
    {
        Task<IEnumerable<PlantResultItem>> GetNotPosted();
        Task<PreparedPostResultItem?> GetPrepared(int plantId);
        Task<CreatePostResult> Post(int plantId, decimal price);
        Task<AddPlantResult> Create(string Name, string Description,
            int[] Regions, int SoilId,
            int GroupId, DateTime Created,
            byte[][] Pictures);

        Task Edit(int PlantId, string Name, string Description,
                int[] Regions, int SoilId,
            int GroupId);
    }
}
