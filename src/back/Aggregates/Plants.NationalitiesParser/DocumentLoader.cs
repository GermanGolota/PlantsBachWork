namespace Plants.NationalitiesParser;

internal sealed class DocumentLoader
{
    private readonly IHttpClientFactory _clientFactory;

    public DocumentLoader(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<HtmlDocument> LoadDocumentWithNationalities()
    {
        var client = _clientFactory.CreateClient();
        var result = await client.GetAsync("https://www.worldatlas.com/articles/what-is-a-demonym-a-list-of-nationalities.html");
        result.EnsureSuccessStatusCode();
        var html = await result.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }
}
