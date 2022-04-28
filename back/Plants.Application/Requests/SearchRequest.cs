using MediatR;
using System;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record SearchRequest(string? PlantName,
        decimal? LowerPrice,
        decimal? TopPrice,
        DateTime? LastDate,
        int[]? GroupIds,
        int[]? RegionIds,
        int[]? SoilIds) : IRequest<SearchResult>;

    public record SearchResult(List<SearchResultItem> Items);
    public record SearchResultItem(int Id, string PlantName, string Description, int[] ImageIds)
    {
        //user by converter
        public SearchResultItem() : this(0, "", "", null)
        {

        }
    }
}
