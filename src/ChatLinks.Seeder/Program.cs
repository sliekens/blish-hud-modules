using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SL.ChatLinks;
using SL.ChatLinks.Integrations;
using SL.Common;

ServicePointManager.DefaultConnectionLimit = int.MaxValue;
Sqlite3Setup.Run();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        _ = logging.AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Warning);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddGw2Client();
        services.AddStaticDataClient();
        services.AddDatabase(options =>
        {
            string directory = hostContext.Configuration["Database:Directory"];
            options.Directory = Path.GetFullPath(directory);
        });

        _ = services.AddSingleton<IEventAggregator, DefaultEventAggregator>();

        _ = services.AddSingleton<ILocale, DefaultLocale>();

        _ = services.AddSingleton<DatabaseSeeder>();
    })
    .Build();

using DatabaseSeeder seeder = host.Services.GetRequiredService<DatabaseSeeder>();
await seeder.SeedAll().ConfigureAwait(false);
