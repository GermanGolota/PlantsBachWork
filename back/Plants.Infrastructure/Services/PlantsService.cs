using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Commands;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<PlantResultDto> GetBy(int id)
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM plants_v WHERE Id = @id";
                    var p = new
                    {
                        id
                    };
                    var items = await connection.QueryAsync<PlantResultDto>(sql, p);
                    var first = items.FirstOrDefault();
                    PlantResultDto res;
                    if (first == default)
                    {
                        res = null;
                    }
                    else
                    {
                        res = first;
                    }
                    return res;
                }
            }
        }

        public async Task<IEnumerable<PlantResultItem>> GetNotPosted()
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT id, plant_name, description, is_mine FROM plants_v";
                    return await connection.QueryAsync<PlantResultItem>(sql);
                }
            }
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
                        (false, 4) => "You cannot post plant that is in a planning stage",
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

        public async Task Edit(int PlantId, string Name, string Description,
            int[] Regions, int SoilId, int GroupId, int[] RemovedImages, byte[][] NewImages)
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "call edit_plant(@PlantId, @Name, @Description, @Regions, @SoilId, @GroupId, @RemovedImages, @NewImages)";
                    var p = new
                    {
                        PlantId,
                        Name,
                        Description,
                        Regions,
                        SoilId,
                        GroupId,
                        RemovedImages,
                        NewImages
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }
    }
}
