using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Plants.Domain.Infrastructure.Config;
using Plants.Services.Infrastructure.Encryption;
using System.Net.Http.Json;
using System.Text;

namespace Plants.Aggregates.Infrastructure.Helper.ElasticSearch;

public class ElasticSearchHelper
{
    private readonly ConnectionConfig _options;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<ElasticSearchUserUpdater> _logger;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;

    public ElasticSearchHelper(IOptions<ConnectionConfig> options, IHttpClientFactory clientFactory, ILogger<ElasticSearchUserUpdater> logger, IIdentityProvider identity, SymmetricEncrypter encrypter)
    {
        _options = options.Value;
        _clientFactory = clientFactory;
        _logger = logger;
        _identity = identity;
        _encrypter = encrypter;
    }

    public HttpClient GetClient()
    {
        var client = _clientFactory.CreateClient();
        var identity = _identity.Identity!;
        var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{identity.UserName}:{_encrypter.Decrypt(identity.Hash)}"));
        client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.Authorization, $"Basic {svcCredentials}");
        return client;
    }

    public Uri GetUrl(string path)
    {
        var identity = _identity.Identity!;
        var baseUrl = new Uri(string.Format(_options.ElasticSearchConnectionTemplate, identity.UserName, _encrypter.Decrypt(identity.Hash)));
        var builder = new UriBuilder(baseUrl);
        builder.Path = path;
        return builder.Uri;
    }

    public async Task HandleCreationAsync<T>(string objectType, string objectName, HttpResponseMessage result, Func<T, bool> isValid)
    {
        if (result.IsSuccessStatusCode)
        {
            var resultObject = await result.Content.ReadFromJsonAsync<T>();
            if (resultObject is null || isValid(resultObject) is false)
            {
                var message = await result.Content.ReadAsStringAsync();
                _logger.LogInformation("Did not create '{type}' '{value}' with message - '{msg}'", objectType, objectName, message);
            }
        }
        else
        {
            var message = await result.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create '{type}' '{value}' with message - '{msg}'", objectType, objectName, message);
            throw new Exception($"Failed to create '{objectType}' '{objectName}' with message - '{message}'");
        }
    }

}
