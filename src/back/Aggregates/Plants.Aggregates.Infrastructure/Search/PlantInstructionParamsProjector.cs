using Nest;

namespace Plants.Aggregates.Infrastructure;

internal class PlantInstructionParamsProjector : ISearchParamsProjector<PlantInstruction, PlantInstructionParams>
{
    public SearchDescriptor<PlantInstruction> ProjectParams(PlantInstructionParams parameters, SearchDescriptor<PlantInstruction> desc) =>
        desc.Query(q => q.Bool(b => b.Must(
            /*u => u.Term(f => f.Field(_ => _.Information.GroupName).Value(parameters.GroupName)),*/
            u => u.FilterOrAll(parameters.Title, (qo, filter) => qo.Fuzzy(f => f.Field(_ => _.Information.Title).Value(filter))),
            u => u.FilterOrAll(parameters.Description, (qo, filter) => qo.Fuzzy(f => f.Field(_ => _.Information.Description).Value(filter)))
            )));
}
