using MediatR;

namespace Plants.Application.Requests;

public record SearchRequest(string? PlantName,
    decimal? LowerPrice,
    decimal? TopPrice,
    DateTime? LastDate,
    long[]? GroupIds,
    long[]? RegionIds,
    long[]? SoilIds) : IRequest<SearchResult>;

public record SearchResult(List<SearchResultItem> Items);
public record SearchResultItem(long Id, string PlantName, string Description, long[] ImageIds, double Price)
{
    //used by converter
    public SearchResultItem() : this(0, "", "", null, 0)
    {

    }
}
