namespace Plants.Aggregates;

internal sealed class SearchInstructionsHandler : IRequestHandler<SearchInstructions, IEnumerable<FindInstructionsViewResultItem>>
{
    private readonly ISearchQueryService<PlantInstruction, PlantInstructionParams> _search;

    public SearchInstructionsHandler(ISearchQueryService<PlantInstruction, PlantInstructionParams> search)
    {
        _search = search;
    }

    public async Task<IEnumerable<FindInstructionsViewResultItem>> Handle(SearchInstructions request, CancellationToken token)
    {
        var results = await _search.SearchAsync(request.Parameters, request.Options, token);
        //TODO: Fix group filtering not working with elastic
        return results
            .Where(_ => _.Information.GroupName == request.Parameters.GroupName)
            .Select(result => new FindInstructionsViewResultItem(result.Id, result.Information.Title, result.Information.Description, result.Cover.Location));
    }
}


internal sealed class GetInstructionHandler : IRequestHandler<GetInstruction, GetInstructionViewResultItem?>
{
    private readonly IProjectionQueryService<PlantInstruction> _query;

    public GetInstructionHandler(IProjectionQueryService<PlantInstruction> query)
    {
        _query = query;
    }

    public async Task<GetInstructionViewResultItem?> Handle(GetInstruction request, CancellationToken token)
    {
        GetInstructionViewResultItem? result;
        if (await _query.ExistsAsync(request.InstructionId, token))
        {
            var instruction = await _query.GetByIdAsync(request.InstructionId, token);

            var information = instruction.Information;
            result = new(instruction.Id, information.Title, information.Description, information.Text, instruction.Cover.Location, information.GroupName);
        }
        else
        {
            result = null;
        }
        return result;
    }
}
