using Plants.Initializer;
using System.Globalization;

namespace Plants.NationalitiesParser;

internal static class NationalityParser
{
    public static Nationality[] Parse(HtmlDocument document)
    {
        var table = document.DocumentNode.Descendants("table").First();
        var body = table.Descendants("tbody").First();
        var rows = body.Descendants("tr");
        var regions = GetRegions();
        return rows.Select(row =>
        {
            var data = row.Descendants("td").ToArray();
            if (data.Length != 2)
            {
                throw new NotImplementedException($"Table have updated, need to update the parser. Failed at '{String.Join(", ", data.Select(_ => _.InnerHtml))}'");
            }

            var name = data[0].InnerHtml.Trim();
            if(name.Contains('[') && name.Contains(']'))
            {
                var indexStart = name.IndexOf('[');
                name = name.Substring(0, indexStart);
            }
            var denonym = data[1].InnerHtml.Trim();
            var isoCode = TryGetIsoCode(name, regions);
            if (isoCode is not null)
            {
                return new Nationality(isoCode, name, denonym);
            }
            else
            {
                Console.WriteLine($"Failed to find iso code for '{name}'");
                return null;
                //throw new FormatException();
            }
        })
            .OfType<Nationality>()
            .GroupBy(_ => _.TwoLetterIsoCode)
            .Select(_ => _.First())
            .ToArray();
    }

    private static RegionInfo[] GetRegions() =>
        CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(culture =>
            {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Message);
                    return null;
                }
            })
            .OfType<RegionInfo>()
            .ToArray();

    private static string? TryGetIsoCode(string name, RegionInfo[] regions)
    {
        string? result = null;

        foreach (var region in regions)
        {
            if (region.EnglishName.Contains(name) || region.DisplayName.Contains(name))
            {
                result = region.TwoLetterISORegionName;
                break;
            }
        }

        return result;
    }
}
