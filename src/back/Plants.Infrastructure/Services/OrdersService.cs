using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plants.Application.Commands;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    internal class OrdersService : IOrdersService
    {
        private readonly PlantsContextFactory _ctx;
        private readonly ILogger<OrdersService> _logger;

        public OrdersService(PlantsContextFactory ctxFactory, ILogger<OrdersService> logger)
        {
            _ctx = ctxFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<OrdersResultItem>> GetOrders(bool onlyMine)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string viewName = onlyMine switch
                    {
                        true => "current_user_orders",
                        false => "plant_orders_v"
                    };
                    string sql = $"SELECT * FROM {viewName};";
                    return await connection.QueryAsync<OrdersResultItem>(sql);
                }
            }
        }

        public async Task ConfirmStarted(int orderId, string trackingNumber)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = @"INSERT INTO plant_delivery(order_id, delivery_tracking_number)
                            VALUES(@orderId, @trackingNumber);";
                    var p = new
                    {
                        orderId,
                        trackingNumber
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }

        public async Task ConfirmReceived(int deliveryId)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = @"CALL confirm_received(@deliveryId);";
                    var p = new
                    {
                        deliveryId
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }

        public async Task<RejectOrderResult> Reject(int orderId)
        {
            var ctx = _ctx.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = @"DELETE FROM plant_order WHERE post_id = @orderId";
                    var p = new
                    {
                        orderId
                    };
                    bool succ;
                    try
                    {
                        var deleted = await connection.ExecuteAsync(sql, p);
                        succ = deleted == 1;
                    }
                    catch(Exception e)
                    {
                        _logger.LogError(e, "Failed to delete with {0}", e.Message);
                        succ = false;
                    }
                    return new RejectOrderResult(succ);
                }
            }
        }
    }
}
