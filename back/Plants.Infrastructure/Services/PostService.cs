using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Commands;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Infrastructure.Helpers;
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

        public async Task<PostResultItem> GetBy(int postId)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM plant_post_v WHERE id = @postId";
                    var p = new
                    {
                        postId = postId
                    };
                    var res = await connection.QueryAsync<PostResultItem>(sql, p);
                    var first = res.FirstOrDefault();
                    PostResultItem final;
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

        public async Task<PlaceOrderResult> Order(int postId, string city, int postNumber)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM place_order(@postId, @city, @postNumber)";
                    var p = new
                    {
                        postId, city, postNumber
                    };
                    var res = (await connection.QueryAsync<PostResultDb>(sql, p)).FirstOrDefault();
                    var message = (res.WasPlaced, res.ReasonCode) switch
                    {
                        (true, _) => "Successfully Ordered!",
                        (false, 1) => "This plant is not yet posted",
                        (false, 2) => "This plant have already been ordered",
                        (false, _) => "Failed to oreder plant!"
                    };
                    return new PlaceOrderResult(res.WasPlaced, message);
                }
            }
        }

        internal record PostResultDb(bool WasPlaced, int ReasonCode);
    }
}
