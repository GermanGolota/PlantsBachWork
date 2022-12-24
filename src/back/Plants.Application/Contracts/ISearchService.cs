using Plants.Application.Requests;

namespace Plants.Application.Contracts;

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
