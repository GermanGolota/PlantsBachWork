namespace Plants.Aggregates.Infrastructure.Helper.ElasticSearch;

public class RoleDefinition
{
    public required List<string> Cluster { get; set; } = new();
    public required List<RoleDefinitionIndices> Indices { get; set; }
}

public class RoleDefinitionIndices
{
    public required List<string> Names { get; set; }
    public required List<string> Privileges { get; set; }
}
