﻿using System.ComponentModel.Composition;
using System.Globalization;

using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.Adapters;
using SL.ChatLinks.Integrations;
using SL.ChatLinks.Logging;
using SL.ChatLinks.Storage;
using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common;

using SQLitePCL;

namespace SL.ChatLinks;

[Export(typeof(Blish_HUD.Modules.Module))]
[method: ImportingConstructor]
public class Module([Import("ModuleParameters")] ModuleParameters parameters) : Blish_HUD.Modules.Module(parameters)
{
    private IEventAggregator? _eventAggregator;

    private ServiceProvider? _serviceProvider;

    private ModuleSettings? _moduleSettings;

    protected override void DefineSettings(SettingCollection settings)
    {
        _moduleSettings = new ModuleSettings(settings);
    }

    protected override void Initialize()
    {
        if (_moduleSettings is null)
        {
            throw new InvalidOperationException("Module settings not defined.");
        }

        ServiceCollection services = new();
        services.AddSingleton(ModuleParameters);
        services.AddSingleton(_moduleSettings);
        services.ConfigureOptions(_moduleSettings);
        services.AddSingleton<IOptionsChangeTokenSource<ChatLinkOptions>>(_moduleSettings);

        services.AddSingleton<ILocale, OverlayLocale>();
        services.AddSingleton<IIntrospection, ModuleIntrospection>();
        services.AddGw2Client();

        services.AddDatabase(options =>
        {
            options.Directory = ModuleParameters.DirectoriesManager.GetFullDirectoryPath("chat-links-data");
        });

        services.AddSingleton<DatabaseSeeder>();

        services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
        services.AddTransient<MainIcon>();
        services.AddTransient<MainIconViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ItemsTabViewFactory>();
        services.AddTransient<ItemsTabViewModel>();
        services.AddTransient<ItemsTabViewModelFactory>();
        services.AddTransient<ItemsListViewModelFactory>();
        services.AddTransient<ItemTooltipViewModelFactory>();
        services.AddTransient<ChatLinkEditorViewModelFactory>();
        services.AddTransient<UpgradeEditorViewModelFactory>();
        services.AddTransient<UpgradeSelectorViewModelFactory>();
        services.AddTransient<ItemSearch>();
        services.AddSingleton<Customizer>();
        services.AddSingleton<Hero>();
        services.AddHttpClient<ItemIcons>();
        services.AddTransient<IClipBoard, WpfClipboard>();

        services.AddLogging(builder =>
        {
            builder.Services.AddSingleton<ILoggerProvider, LoggingAdapterProvider<Module>>();
            if (ApplicationSettings.Instance.DebugEnabled || GameService.Debug.EnableDebugLogging.Value)
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddFilter("System", LogLevel.Information);
                builder.AddFilter("Microsoft", LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            }
            else
            {
                builder.AddFilter("System", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Critical);
            }
        });

        services.AddSingleton<ITokenProvider, Gw2SharpTokenProvider>();

        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        _serviceProvider = services.BuildServiceProvider();
        _eventAggregator = _serviceProvider.GetRequiredService<IEventAggregator>();
        SetupSqlite3();
    }

    protected override async Task LoadAsync()
    {
        var databaseManager = _serviceProvider.GetRequiredService<DatabaseSeeder>();
        var locale = _serviceProvider.GetRequiredService<ILocale>();
        await databaseManager.Migrate(locale.Current);

        _ = _serviceProvider.GetRequiredService<MainIcon>();
        _ = _serviceProvider.GetRequiredService<MainWindow>();

        await databaseManager.Sync(locale.Current, CancellationToken.None);
    }

    private static void SetupSqlite3()
    {
        SQLite3Provider_dynamic_cdecl.Setup("e_sqlite3", new ModuleGetFunctionPointer("sliekens.e_sqlite3"));
        raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
    }

    protected override void Unload()
    {
        _eventAggregator?.Publish(new ModuleUnloading());
        _serviceProvider?.Dispose();
    }
}