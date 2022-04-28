using Plants.Application.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface ISearchService
    {
        Task<IEnumerable<SearchResultItem>> Search(string? PlantName,
        decimal? LowerPrice,
        decimal? TopPrice,
        DateTime? LastDate,
        int[] GroupIds,
        int[] RegionIds,
        int[] SoilIds);
    }
}
