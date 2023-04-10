using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plants.NationalitiesParser;

var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services
                .BindConfigSections(ctx.Configuration)
                .AddShared();

            services.AddHttpClient();

            services.AddSingleton<DocumentLoader>();
        })
        .Build();

var loader = host.Services.GetRequiredService<DocumentLoader>();
var document = await loader.LoadDocumentWithNationalities();
var nationalities = NationalityParser.Parse(document);
var code = CodeWriter.WriteInitializedDictionary(nationalities);
Console.WriteLine(code);