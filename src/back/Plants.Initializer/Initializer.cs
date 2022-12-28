using Microsoft.Extensions.Logging;

namespace Plants.Initializer;

internal class Initializer
{
    private readonly MongoRolesDbInitializer _mongo;
    private readonly ElasticSearchRolesInitializer _elasticSearch;
    private readonly AdminUserCreator _userCreator;
    private readonly ILogger<Initializer> _logger;

    public Initializer(MongoRolesDbInitializer mongo, ElasticSearchRolesInitializer elasticSearch, AdminUserCreator userCreator, ILogger<Initializer> logger)
    {
        _mongo = mongo;
        _elasticSearch = elasticSearch;
        _userCreator = userCreator;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting initialization");
        await _mongo.InitializeAsync();
        _logger.LogInformation("Created roles in mongo db");
        await _elasticSearch.InitializeAsync();
        _logger.LogInformation("Created roles in elastic search");
        var userCreateResult = await _userCreator.SendCreateAdminCommandAsync();
        userCreateResult.Match(
            _ => _logger.LogInformation("Sucessfully created user!"),
            fail => throw new Exception(String.Join(", ", fail.Reasons))
            );
        var passwordResetResult = await _userCreator.SendResetPasswordCommandAsync();
        passwordResetResult.Match(
            _ => _logger.LogInformation("Sucessfully changed password!"),
            fail => throw new Exception(String.Join(", ", fail.Reasons))
            );

        _logger.LogInformation("Successfully initialized");
    }

}
