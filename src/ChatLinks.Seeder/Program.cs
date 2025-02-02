using System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SL.ChatLinks;
using SL.ChatLinks.Integrations;
using SL.Common;

using SQLitePCL;

ServicePointManager.DefaultConnectionLimit = int.MaxValue;
Batteries_V2.Init();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddGw2Client();
        services.AddDatabase(options =>
        {
            string directory = hostContext.Configuration["Database:Directory"];
            options.Directory = Path.GetFullPath(directory);
        });

        services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
        services.AddSingleton<IIntrospection, NullIntrospection>();

        services.AddSingleton<DatabaseSeeder>();
    })
    .Build();

using var seeder = host.Services.GetRequiredService<DatabaseSeeder>();
await seeder.SeedAll();