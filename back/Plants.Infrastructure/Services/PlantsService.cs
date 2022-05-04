using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Commands;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Core.Entities;
using Plants.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    public class PlantsService : IPlantsService
    {
        private readonly PlantsContextFactory _ctxFactory;

        public PlantsService(PlantsContextFactory ctxFactory)
        {
            _ctxFactory = ctxFactory;
        }

        public async Task<IEnumerable<PlantResultItem>> GetNotPosted()
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                string sql = "SELECT * FROM plants_v";
                var items = await ctx.PlantsVs.FromSqlRaw(sql).ToListAsync();
                return items.Select(Convert);
            }
        }

        private PlantResultItem Convert(PlantsV plant)
        {
            return new PlantResultItem(plant.Id!.Value, plant.PlantName, plant.Description, plant.Ismine ?? false);
        }

        public async Task<PreparedPostResultItem> GetPrepared(int plantId)
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM prepared_for_post_v WHERE id = @plantId";
                    var p = new
                    {
                        plantId
                    };
                    var res = await connection.QueryAsync<PreparedPostResultItem>(sql, p);
                    var first = res.FirstOrDefault();
                    PreparedPostResultItem final;
                    if (first != default)
                    {
                        final = first;
                    }
                    else
                    {
                        final = null;
                    }
                    return final;
                }
            }
        }

        public async Task<CreatePostResult> Post(int plantId, decimal price)
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM post_plant(@plantId, @price)";
                    var p = new
                    {
                        plantId,
                        price
                    };
                    var res = (await connection.QueryAsync<PostService.PostResultDb>(sql, p)).FirstOrDefault();
                    var message = (res.WasPlaced, res.ReasonCode) switch
                    {
                        (true, _) => "Successfully Posted!",
                        (false, 1) => "This plant does not exist",
                        (false, 2) => "This plant have already been posted",
                        (false, 3) => "Price cannot have this value!",
                        (false, _) => "Failed to post plant!"
                    };
                    return new CreatePostResult(res.WasPlaced, message);
                }
            }
        }

        public async Task<AddPlantResult> Create(string Name, string Description, int[] Regions,
            int SoilId, int GroupId, DateTime Created, byte[][] Pictures)
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT create_plant(@Name, @Description, " +
                        "@Regions, @SoilId, @GroupId, @Created, @Pictures);";
                    var p = new
                    {
                        Name,
                        Description,
                        Regions,
                        SoilId,
                        GroupId,
                        Created,
                        Pictures
                    };
                    var res = await connection.QueryAsync<int>(sql, p);
                    var first = res.FirstOrDefault();
                    return new AddPlantResult(first);
                }
            }
        }

        public async Task Edit(int PlantId, string Name, string Description, int[] Regions, int SoilId, int GroupId)
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "call edit_plant(@PlantId, @Name, @Description, @Regions, @SoilId, @GroupId)";
                    var p = new
                    {
                        PlantId, Name, Description, Regions, SoilId, GroupId
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }
    }
}
