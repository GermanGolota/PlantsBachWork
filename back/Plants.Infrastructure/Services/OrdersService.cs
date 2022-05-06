using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    internal class OrdersService : IOrdersService
    {
        private readonly PlantsContextFactory _ctx;

        public OrdersService(PlantsContextFactory ctxFactory)
        {
            _ctx = ctxFactory;
        }

        public async Task<IEnumerable<OrdersResultItem>> GetOrders()
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM plant_orders_v;";
                    return await connection.QueryAsync<OrdersResultItem>(sql);
                }
            }
        }
    }
}
