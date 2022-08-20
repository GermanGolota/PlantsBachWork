using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using Plants.Core.Entities;
using Plants.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    public class StatsService : IStatsService
    {
        private readonly PlantsContextFactory _contextFactory;

        public StatsService(PlantsContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<GroupFinancialStats>> GetFinancialIn(DateTime from, DateTime to)
        {
            var ctx = _contextFactory.CreateDbContext();
            await using (ctx)
            {
                using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM get_financial(@from, @to)";
                    var p = new
                    {
                        from,
                        to
                    };
                    var result = await connection.QueryAsync<GroupFinancialStats>(sql, p);
                    return result;
                }
            }
        }

        public async Task<IEnumerable<GroupTotalStats>> GetTotals()
        {
            var ctx = _contextFactory.CreateDbContext();
            await using (ctx)
            {
                string sql = "SELECT * FROM plant_stats_v";
                var result = await ctx.PlantStatsVs.FromSqlRaw(sql)
                    .ToListAsync();
                return result.Select(Map);
            }
        }

        private static GroupTotalStats Map(PlantStatsV x)
        {
            return new GroupTotalStats(x.Id.Value, x.GroupName, x.Income.Value, x.Instructions.Value, x.Popularity.Value);
        }
    }
}
