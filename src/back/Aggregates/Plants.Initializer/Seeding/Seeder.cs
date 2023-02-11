using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;

namespace Plants.Initializer;

internal class Seeder
{
    private readonly SeedingConfig _options;
    private readonly CommandHelper _command;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<Seeder> _logger;
    private readonly IIdentityProvider _identity;
    private readonly IIdentityHelper _helper;
    private readonly TempPasswordContext _context;
    private readonly UserConfig _userOptions;

    public Seeder(IOptions<SeedingConfig> options, CommandHelper command, 
        IDateTimeProvider dateTime, ILogger<Seeder> logger, 
        IOptionsSnapshot<UserConfig> userOptions, IIdentityProvider identity, 
        IIdentityHelper helper, TempPasswordContext context)
    {
        _options = options.Value;
        _command = command;
        _dateTime = dateTime;
        _logger = logger;
        _identity = identity;
        _helper = helper;
        _context = context;
        _userOptions = userOptions.Get(UserConstrants.NewAdmin);
    }

    public async Task SeedAsync(CancellationToken token)
    {
        if (_options.ShouldSeed)
        {
            SetupIdentity();

            _logger.LogInformation("Starting seeding process");
            var rng = new Random();
            var testData = await LoadTestDataAsync(token);
            var images = await LoadTestImagesAsync(token);
            var users = Enumerable.Range(0, _options.UsersCount)
                .Select(_ => new UserCreationDto(
                    Faker.Name.First(),
                    Faker.Name.Last(),
                    Faker.Phone.Number(),
                    Faker.Internet.UserName(),
                    Faker.Internet.Email(),
                    testData.Languages.Random(),
                    Enum.GetValues<UserRole>().Random(1, 3).ToArray()
                    ))
                .ToArray();
            var stocks = Enumerable.Range(0, _options.PlantsCount)
                .Select(_ =>
                {
                    var groupNames = testData.Groups.Random(1, 3);
                    var name = $"{Faker.Country.Name()} {groupNames.First()}";
                    return new PlantInformation(name,
                        Faker.Lorem.Sentence(5),
                        testData.Regions.Random(3).ToArray(),
                        testData.Soils.Random(1, 3).ToArray(),
                        groupNames.ToArray());
                })
                .ToArray();

            var results = new List<OneOf<CommandAcceptedResult, CommandForbidden>>();
            foreach (var user in users)
            {
                results.Add(await _command.SendAndWaitAsync(
                    factory => factory.Create<CreateUserCommand, User>(user.Login.ToGuid()),
                    meta => new CreateUserCommand(meta, user),
                    token)
                    );

                results.Add(await _command.SendAndWaitAsync(
                    factory => factory.Create<ChangePasswordCommand, User>(user.Login.ToGuid()),
                    meta => new ChangePasswordCommand(meta, user.Login, 
                    _context.TempPassword, $"{user.FirstName.ToLower().Trim()}password"),
                    token)
                    );
            }

            List<Guid> stockIds = new();
            foreach (var stock in stocks)
            {
                var stockId = rng.GetRandomConvertableGuid();
                stockIds.Add(stockId);
                results.Add(await _command.SendAndWaitAsync(
                    factory => factory.Create<AddToStockCommand, PlantStock>(stockId),
                    meta => new AddToStockCommand(meta, stock, _dateTime.UtcNow, images.Random(1, 3).ToArray()),
                    token)
                    );
            }

            var stocksToPost = stockIds.Random((int)(stocks.Length * 3.0 / 4));
            foreach (var postId in stocksToPost)
            {
                results.Add(await _command.SendAndWaitAsync(
                   factory => factory.Create<PostStockItemCommand, PlantStock>(postId),
                   meta => new PostStockItemCommand(meta, rng.Next(_options.PriceRangeMin, _options.PriceRangeMax)),
                   token)
                   );
            }

            var (sucesses, failures) = results.Split();
            _logger.LogInformation("Successfully created '{successCount}' entities and failed to create '{failCount}'", sucesses.Count(), failures.Count());
            foreach (var failure in failures)
            {
                _logger.LogInformation("Failed to create entity stating '{reasons}'", failure.Reasons);
            }
            _logger.LogInformation("Seeding completed");
        }
        else
        {
            _logger.LogInformation("Skiping seeding");
        }
    }

    public void SetupIdentity()
    {
        _identity.UpdateIdentity(_helper.Build(_userOptions.Password, _userOptions.Username, _identity.Identity!.Roles));
    }

    private async Task<byte[][]> LoadTestImagesAsync(CancellationToken token)
    {
        var path = Path.Combine("Seeding", "Data", "Images");
        var loadTasks = Directory.GetFiles(path).Select(file => File.ReadAllBytesAsync(file, token));
        return await Task.WhenAll(loadTasks);
    }

    private async Task<PlantTestData> LoadTestDataAsync(CancellationToken token)
    {
        var str = await File.ReadAllTextAsync(Path.Combine("Seeding", "Data", "TestData.json"), token);
        return JsonConvert.DeserializeObject<PlantTestData>(str)!;
    }
}

internal record PlantTestData(List<string> Regions, List<string> Groups, List<string> Soils, List<string> Languages);