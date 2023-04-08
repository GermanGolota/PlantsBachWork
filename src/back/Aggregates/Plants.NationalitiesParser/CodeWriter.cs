using Plants.Initializer;
using System.Text;

namespace Plants.NationalitiesParser;

internal static class CodeWriter
{
    public static string WriteInitializedDictionary(Nationality[] initialValues)
    {
        var sb = new StringBuilder();

        sb.AppendLine("private static readonly Dictionary<string, Nationality> _isoToNationality = new()");
        sb.AppendLine("{");
        foreach (var nationality in initialValues)
        {
            sb.AppendIniter(nationality);
        }
        sb.AppendLine("};");

        return sb.ToString();
    }

    private static StringBuilder AppendIniter(this StringBuilder sb, Nationality nationality) =>
        sb.Append("\t{ ")
        .AppendStringLiteral(nationality.TwoLetterIsoCode)
        .Append(", ")
        .Append($"new {nameof(Nationality)}(")
        .AppendStringLiteral(nationality.TwoLetterIsoCode)
        .Append(", ")
        .AppendStringLiteral(nationality.FullName)
        .Append(", ")
        .AppendStringLiteral(nationality.Demonym)
        .Append(") },")
        .AppendLine();

    private static StringBuilder AppendStringLiteral(this StringBuilder sb, string value)
    {
        sb.Append("\"");
        sb.Append(value);
        sb.Append("\"");

        return sb;
    }
}
