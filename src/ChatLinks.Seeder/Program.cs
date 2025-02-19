using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SL.ChatLinks;
using SL.ChatLinks.Integrations;
using SL.Common;

using SQLitePCL;

ServicePointManager.DefaultConnectionLimit = int.MaxValue;
Batteries_V2.Init();

IHost host = Host.CreateDefaultBuilder(args)
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

        _ = services.AddSingleton<DatabaseSeeder>();
    })
    .Build();

using DatabaseSeeder seeder = host.Services.GetRequiredService<DatabaseSeeder>();
await seeder.SeedAll().ConfigureAwait(false);
