using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using System.Linq;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly PlantsContextFactory _ctx;

        public PostService(PlantsContextFactory ctx)
        {
            _ctx = ctx;
        }

        public async Task<PostResultItem> GetBy(int orderId)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM plant_post_v WHERE id = @orderId";
                    var p = new
                    {
                        orderId = orderId
                    };
                    var res = await connection.QueryAsync<PostResultItem>(sql, p);
                    var first = res.FirstOrDefault();
                    PostResultItem final;
                    if(first != default)
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
    }
}
