using System.ComponentModel.Composition;

using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.Adapters;
using SL.ChatLinks.Integrations;
using SL.ChatLinks.Logging;
using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Achievements;
using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;

using SQLitePCL;

namespace SL.ChatLinks;

[Export(typeof(Module))]
[method: ImportingConstructor]
public class ChatLinksModule([Import("ModuleParameters")] ModuleParameters parameters) : Module(parameters)
{
    private IEventAggregator? _eventAggregator;

    private ServiceProvider? _serviceProvider;

    private ModuleSettings? _moduleSettings;

    private readonly Clock _clock = new();

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

        #region Settings

        _ = services.AddSingleton(ModuleParameters);
        _ = services.AddSingleton(_moduleSettings);
        _ = services.ConfigureOptions(_moduleSettings);
        _ = services.AddSingleton<IOptionsChangeTokenSource<ChatLinkOptions>>(_moduleSettings);

        #endregion Settings

        #region Persistence

        services.AddDatabase(options =>
        {
            options.Directory = ModuleParameters.DirectoriesManager.GetFullDirectoryPath("chat-links-data");
        });

        _ = services.AddSingleton<DatabaseSeeder>();

        #endregion Persistence

        #region Localization

        _ = services.AddSingleton<ILocale, OverlayLocale>();
        _ = services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        #endregion Localization

        #region Web services

        services.AddGw2Client();
        services.AddStaticDataClient();
        _ = services.AddSingleton<ITokenProvider, Gw2SharpTokenProvider>();

        #endregion Web services

        #region Supportive services

        _ = services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
        _ = services.AddTransient<IClipBoard, WpfClipboard>();

        #endregion Supportive services

        #region Logging

        _ = services.AddLogging(builder =>
        {
            _ = builder.Services.AddSingleton<ILoggerProvider, LoggingAdapterProvider<ChatLinksModule>>();
            if (ApplicationSettings.Instance.DebugEnabled || GameService.Debug.EnableDebugLogging.Value)
            {
                _ = builder.SetMinimumLevel(LogLevel.Debug);
                _ = builder.AddFilter("System", LogLevel.Information);
                _ = builder.AddFilter("Microsoft", LogLevel.Information);
                _ = builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            }
            else
            {
                _ = builder.AddFilter("System", LogLevel.Warning);
                _ = builder.AddFilter("Microsoft", LogLevel.Warning);
                _ = builder.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Critical);
            }
        });

        #endregion Logging

        #region UI

        // Corner icon
        _ = services.AddTransient<MainIcon>();
        _ = services.AddTransient<MainIconViewModel>();

        // Main window
        _ = services.AddTransient<MainWindow>();
        _ = services.AddTransient<MainWindowViewModel>();

        // Items tab
        _ = services.AddTransient<ItemsTabViewModelFactory>();
        _ = services.AddTransient<ItemsListViewModelFactory>();
        _ = services.AddTransient<ItemTooltipViewModelFactory>();
        _ = services.AddTransient<ChatLinkEditorViewModelFactory>();
        _ = services.AddTransient<UpgradeEditorViewModelFactory>();
        _ = services.AddTransient<UpgradeSelectorViewModelFactory>();
        _ = services.AddTransient<ItemSearch>();
        _ = services.AddSingleton<Customizer>();
        _ = services.AddSingleton<AccountUnlocks>();
        _ = services.AddHttpClient<ItemIcons>();

        // Achievements tab
        _ = services.AddTransient<AchievementsTabViewModelFactory>();
        _ = services.AddTransient<AchievementTileViewModelFactory>();

        #endregion

        _serviceProvider = services.BuildServiceProvider();
        _eventAggregator = _serviceProvider.GetRequiredService<IEventAggregator>();
        SetupSqlite3();
    }

    protected override async Task LoadAsync()
    {
        ILogger<ChatLinksModule> logger = _serviceProvider.GetRequiredService<ILogger<ChatLinksModule>>();
        ILocale locale = _serviceProvider.GetRequiredService<ILocale>();
        DatabaseSeeder seeder = _serviceProvider.GetRequiredService<DatabaseSeeder>();

        try
        {
            await seeder.Migrate(locale.Current).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Database migration failed, starting with potentially invalid database schema.");
        }

        _ = _serviceProvider.GetRequiredService<MainIcon>();
        _ = _serviceProvider.GetRequiredService<MainWindow>();

        try
        {
            await seeder.Sync(locale.Current, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Database sync failed, starting with potentially stale data.");
        }

        _clock.HourStarted += OnHourStarted;
    }

    private void OnHourStarted(object sender, EventArgs e)
    {
        _eventAggregator?.Publish(new HourStarted());
    }

    private static void SetupSqlite3()
    {
        SQLite3Provider_dynamic_cdecl.Setup("e_sqlite3", new ModuleGetFunctionPointer("sliekens.e_sqlite3"));
        raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
    }

    protected override void Unload()
    {
        _eventAggregator?.Publish(new ModuleUnloading());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clock.Dispose();
            _serviceProvider?.Dispose();
            _moduleSettings?.Dispose();
        }

        base.Dispose(disposing);
    }
}
