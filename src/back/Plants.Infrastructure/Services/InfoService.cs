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
    public class InfoService : IInfoService
    {
        private readonly PlantsContextFactory _ctxFactory;

        public InfoService(PlantsContextFactory ctxFactory)
        {
            _ctxFactory = ctxFactory;
        }

        public async Task<DictsResult> GetDicts()
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                string sql = "SELECT * FROM dicts_v";
                var items = await ctx.DictsVs.FromSqlRaw(sql).ToListAsync();
                Dictionary<int, string> soils = null;
                Dictionary<int, string> regions = null;
                Dictionary<int, string> groups = null;
                foreach (var item in items)
                {
                    switch (item.Type)
                    {
                        case "group":
                            groups = Convert(item);
                            break;

                        case "soil":
                            soils = Convert(item);
                            break;

                        case "region":
                            regions = Convert(item);
                            break;
                    }
                }
                return new DictsResult(groups, regions, soils);
            }
        }

        private static Dictionary<int, string> Convert(DictsV dict)
        {
            return dict.Ids
                .Zip(dict.Values)
                .ToDictionary(x => x.First, x => x.Second);
        }


        public async Task<IEnumerable<PersonAddress>> GetMyAddresses()
        {
            var ctx = _ctxFactory.CreateDbContext();
            await using (ctx)
            {
                var sql = "SELECT * FROM current_user_addresses";
                var res = await ctx.CurrentUserAddresses.FromSqlRaw(sql).ToListAsync();
                var first = res.FirstOrDefault();
                IEnumerable<PersonAddress> output;
                if (first == default)
                {
                    output = new List<PersonAddress>();
                }
                else
                {
                    output = first.Posts.Zip(first.Cities)
                        .Select(pair => new PersonAddress(pair.Second, pair.First));
                }
                return output;
            }
        }
    }
}
