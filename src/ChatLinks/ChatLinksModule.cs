using System.ComponentModel.Composition;

using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Settings;

using GuildWars2;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.Adapters;
using SL.Adapters.Logging;
using SL.ChatLinks.Integrations;
using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Achievements;
using SL.ChatLinks.UI.Tabs.Achievements.Tooltips;
using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common.Progression;

namespace SL.ChatLinks;

[Export(typeof(Module))]
[method: ImportingConstructor]
public class ChatLinksModule([Import("ModuleParameters")] ModuleParameters parameters) : Module(parameters)
{
    private IEventAggregator? _eventAggregator;

    private ServiceProvider? _serviceProvider;

    private ModuleSettings? _moduleSettings;

    private MumbleListener? _listener;

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
        _ = services.AddSingleton<CurrentAccount>();
        _ = services.AddSingleton<AchievementsProgress>();
        _ = services.AddSingleton<AccountBank>();
        _ = services.AddSingleton<AccountMaterialStorage>();
        _ = services.AddSingleton<UnlockedDyes>();
        _ = services.AddSingleton<UnlockedFinishers>();
        _ = services.AddSingleton<UnlockedGliderSkins>();
        _ = services.AddSingleton<UnlockedJadeBotSkins>();
        _ = services.AddSingleton<UnlockedMailCarriers>();
        _ = services.AddSingleton<UnlockedMiniatures>();
        _ = services.AddSingleton<UnlockedMountSkins>();
        _ = services.AddSingleton<UnlockedMistChampionSkins>();
        _ = services.AddSingleton<UnlockedNovelties>();
        _ = services.AddSingleton<UnlockedOutfits>();
        _ = services.AddSingleton<UnlockedRecipes>();
        _ = services.AddSingleton<UnlockedWardrobe>();
        _ = services.AddHttpClient<IconsService>();
        _ = services.AddSingleton<IconsCache>();
        _ = services.AddMemoryCache();

        _ = services.AddTransient(_ => GameLink.Open(name: GameService.Gw2Mumble.CurrentMumbleMapName));
        _ = services.AddTransient<MumbleListener>();

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
        _ = services.AddFactoryDelegate<ItemsTabViewModel.Factory>();
        _ = services.AddFactoryDelegate<ItemsTabViewModel.Factory>();
        _ = services.AddFactoryDelegate<ItemsListViewModel.Factory>();
        _ = services.AddFactoryDelegate<ItemTooltipViewModel.Factory>();
        _ = services.AddFactoryDelegate<ChatLinkEditorViewModel.Factory>();
        _ = services.AddFactoryDelegate<UpgradeEditorViewModel.Factory>();
        _ = services.AddFactoryDelegate<UpgradeSelectorViewModel.Factory>();
        _ = services.AddFactoryDelegate<UpgradeSlotViewModel.Factory>();
        _ = services.AddTransient<ItemSearch>();
        _ = services.AddSingleton<Customizer>();

        // Achievements tab
        _ = services.AddFactoryDelegate<AchievementsTabViewModel.Factory>();
        _ = services.AddFactoryDelegate<AchievementTileViewModel.Factory>();
        _ = services.AddFactoryDelegate<AchievementTooltipViewModel.Factory>();

        #endregion

        _serviceProvider = services.BuildServiceProvider();
        _eventAggregator = _serviceProvider.GetRequiredService<IEventAggregator>();
        Sqlite3Setup.Run();
    }

    protected override async Task LoadAsync()
    {
        ILogger<ChatLinksModule> logger = _serviceProvider.GetRequiredService<ILogger<ChatLinksModule>>();
        ILocale locale = _serviceProvider.GetRequiredService<ILocale>();
        DatabaseSeeder seeder = _serviceProvider.GetRequiredService<DatabaseSeeder>();
        CurrentAccount account = _serviceProvider.GetRequiredService<CurrentAccount>();

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
            await seeder.Optimize(locale.Current, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Database sync failed, starting with potentially stale data.");
        }


        try
        {
            await account.Validate(false, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "One or more account details could not be validated.");
        }

        _clock.HourStarted += OnHourStarted;

        _listener = _serviceProvider.GetRequiredService<MumbleListener>();
        _listener.Start();
    }

    private void OnHourStarted(object sender, EventArgs e)
    {
        _eventAggregator?.Publish(new HourStarted());
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
            _listener?.Dispose();
        }

        base.Dispose(disposing);
    }
}
