using Dapper;
using Microsoft.EntityFrameworkCore;
using Plants.Application.Contracts;
using Plants.Application.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plants.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly PlantsContextFactory _contextFactory;

        public SearchService(PlantsContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<SearchResultItem>> Search(
            string plantName,
            decimal? lowerPrice,
            decimal? topPrice,
            DateTime? lastDate,
            int[] groupIds,
            int[] regionIds,
            int[] soilIds)
        {
            var ctx = _contextFactory.CreateDbContext();
            await using (ctx)
            {
                await using (var connection = ctx.Database.GetDbConnection())
                {
                    string sql = "SELECT * FROM search_plant(@plantName, @lowerPrice, @topPrice, @lastDate, @groupIds, @soilIds, @regionIds)";
                    var p = new
                    {
                        plantName,
                        lowerPrice,
                        topPrice,
                        lastDate,
                        groupIds,
                        regionIds,
                        soilIds
                    };
                    var result = await connection.QueryAsync<SearchResultItem>(sql, p);
                    return result;
                }
            }
        }
    }
}
