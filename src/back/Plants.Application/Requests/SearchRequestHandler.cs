using MediatR;
using Plants.Application.Contracts;

namespace Plants.Application.Requests;

public class SearchRequestHandler : IRequestHandler<SearchRequest, SearchResult>
{
    private readonly ISearchService _service;

    public SearchRequestHandler(ISearchService service)
    {
        _service = service;
    }
    public async Task<SearchResult> Handle(SearchRequest request, CancellationToken cancellationToken)
    {
        var (plantName, priceBottom, priceTop, date, groupIds, regionIds, soilIds) = request;
        var res = await _service.Search(plantName, priceBottom, priceTop, date, groupIds, regionIds, soilIds);
        return new SearchResult(res.ToList());
    }
}
