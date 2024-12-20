﻿using System.ComponentModel.Composition;

using Blish_HUD.Modules;

using CommunityToolkit.Diagnostics;

using GuildWars2;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Integrations;
using SL.ChatLinks.Logging;
using SL.ChatLinks.Storage;
using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Achievements;
using SL.ChatLinks.UI.Tabs.Crafting;
using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items.Services;

using SQLitePCL;

namespace SL.ChatLinks;

[Export(typeof(Blish_HUD.Modules.Module))]
[method: ImportingConstructor]
public class Module([Import("ModuleParameters")] ModuleParameters parameters) : Blish_HUD.Modules.Module(parameters)
{
    private MainIcon? _cornerIcon;

    private MainWindow? _mainWindow;
    private ServiceProvider? _sp;

    protected override void Initialize()
    {
        ServiceCollection services = new();
        services.AddSingleton(ModuleParameters);
        services.AddGw2Client();

        services.AddDbContext<ChatLinksContext>(optionsBuilder =>
        {
            string directory = ModuleParameters.DirectoriesManager.GetFullDirectoryPath("chat-links-data");
            string file = Path.Combine(directory, "data.db");
            string connectionString = $"Data Source={file}";
            optionsBuilder.UseSqlite(connectionString);
        }, ServiceLifetime.Transient);

        services.AddTransient<MainIcon>();
        services.AddTransient<MainWindow>();
        services.AddTransient<ItemsTab>();
        services.AddTransient<ItemsTabView>();
        services.AddTransient<Func<ItemsTabView>>(sp => sp.GetRequiredService<ItemsTabView>);
        services.AddTransient<CraftingTab>();
        services.AddTransient<CraftingView>();
        services.AddTransient<Func<CraftingView>>(sp => sp.GetRequiredService<CraftingView>);
        services.AddTransient<AchievementsTab>();
        services.AddTransient<AchievementsView>();
        services.AddTransient<Func<AchievementsView>>(sp => sp.GetRequiredService<AchievementsView>);
        services.AddTransient<ItemSearch>();

        services.AddLogging(builder =>
        {
            builder.AddProvider(new LoggingAdapterProvider<Module>());
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        _sp = services.BuildServiceProvider();

        Batteries_V2.Init();
    }

    private T Resolve<T>() where T : notnull
    {
        if (_sp is null)
        {
            ThrowHelper.ThrowInvalidOperationException("Service provider is not initialized.");
        }

        return _sp.GetRequiredService<T>();
    }

    protected override async Task LoadAsync()
    {
        _mainWindow = Resolve<MainWindow>();
        _cornerIcon = Resolve<MainIcon>();
        _cornerIcon.Click += CornerIcon_Click;

        await using ChatLinksContext context = Resolve<ChatLinksContext>();
        await context.Database.MigrateAsync();
        Progress<string> reporter = new(report =>
        {
            _cornerIcon.LoadingMessage = report;
        });

        await SeedItems(reporter);

        _cornerIcon.LoadingMessage = null;
    }


    private async Task SeedItems(IProgress<string> progress)
    {
        Gw2Client gw2Client = Resolve<Gw2Client>();
        ChatLinksContext context = Resolve<ChatLinksContext>();
        HashSet<int> index = await gw2Client.Items.GetItemsIndex().ValueOnly();
        var existing = await context.Items.Select(item => item.Id).ToListAsync();
        index.ExceptWith(existing);
        if (index.Count == 0)
        {
            return;
        }

        Progress<BulkProgress> bulkProgress = new(report =>
        {
            progress.Report($"Loading items... ({report.ResultCount} of {report.ResultTotal})");
        });

        List<Item> batch = [];
        await foreach (Item item in gw2Client.Items.GetItemsBulk(index, progress: bulkProgress).ValueOnly())
        {
            if (batch.Count == 100)
            {
                await context.BulkInsertAsync(batch, o =>
                {
                    o.BatchSize = 100;
                    o.InsertKeepIdentity = true;
                });
                batch.Clear();
            }

            batch.Add(item);
        }

        if (batch.Count != 0)
        {
            await context.BulkInsertAsync(batch, o =>
            {
                o.BatchSize = 100;
                o.InsertKeepIdentity = true;
            });
        }
    }

    private void CornerIcon_Click(object sender, EventArgs e)
    {
        _mainWindow?.ToggleWindow();
    }

    protected override void Unload()
    {
        if (_cornerIcon is not null)
        {
            _cornerIcon.Click -= CornerIcon_Click;
        }

        _cornerIcon?.Dispose();
        _mainWindow?.Dispose();
        _sp?.Dispose();
    }
}
