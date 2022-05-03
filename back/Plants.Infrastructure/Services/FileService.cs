using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Contracts;
using Plants.Infrastructure.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly PlantsContextFactory _ctx;

        public FileService(PlantsContextFactory ctx)
        {
            _ctx = ctx;
        }

        public async Task<byte[]> LoadPlantImage(int plantImageId)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT image FROM plant_to_image WHERE relation_id = @imageId";
                    var p = new
                    {
                        imageId = plantImageId
                    };
                    var images = await connection.QueryAsync<byte[]>(sql, p);
                    return images.FirstOrDefault();
                }
            }
        }
    }
}
