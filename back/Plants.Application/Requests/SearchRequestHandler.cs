using MediatR;
using Plants.Application.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Requests
{
    public class SearchRequestHandler : IRequestHandler<SearchRequest, SearchResult>
    {
        private readonly ISearchService _service;

        public SearchRequestHandler(ISearchService service)
        {
            _service = service;
        }
        public async Task<SearchResult> Handle(SearchRequest request, CancellationToken cancellationToken)
        {
            var (a, b, c, d, e, f, g) = request;
            var res = await _service.Search(a, b, c, d, e, f, g);
            return new SearchResult(res.ToList());
        }
    }
}
