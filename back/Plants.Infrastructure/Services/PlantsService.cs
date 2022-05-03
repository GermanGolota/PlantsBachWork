using Microsoft.EntityFrameworkCore;
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
    }
}
