﻿using Dapper;
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
                    string sql = @"INSERT INTO plant_shipment(delivery_id)
                            VALUES(@deliveryId);";
                    var p = new
                    {
                        deliveryId
                    };
                    await connection.ExecuteAsync(sql, p);
                }
            }
        }
    }
}