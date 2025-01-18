using System.ComponentModel.Composition;
using System.IO.Compression;

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Settings;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    private MainIcon? _cornerIcon;

    private MainWindow? _mainWindow;

    private ServiceProvider? _serviceProvider;

    private ContextMenuStripItem? _syncButton;

    private SettingEntry<bool>? _raiseStackSize;

    private SettingEntry<bool>? _bananaMode;

    protected override void DefineSettings(SettingCollection settings)
    {
        _raiseStackSize = settings.DefineSetting(
            "RaiseStackSize",
            false,
            () => "Raise the maximum item stack size from 250 to 255",
            () => "When enabled, you can generate chat links with stacks of 255 items."
        );

        _bananaMode = settings.DefineSetting(
            "BananaMode",
            false,
            () => "Banana of Imagination-mode",
            () => "When enabled, you can add an upgrade component to any item."
        );
    }

    protected override void Initialize()
    {
        ServiceCollection services = new();
        services.AddSingleton(ModuleParameters.SettingsManager.ModuleSettings);
        services.Configure<ChatLinkOptions>(options =>
        {
            options.RaiseStackSize = _raiseStackSize!.Value;
            options.BananaMode = _bananaMode!.Value;
        });

        services.AddSingleton<IOptionsChangeTokenSource<ChatLinkOptions>, ChatLinkOptionsAdapter>();

        services.AddGw2Client();

        services.AddDbContext<ChatLinksContext>(optionsBuilder =>
        {
            string file = DatabaseLocation();
            string connectionString = $"Data Source={file}";
            var connection = new SqliteConnection(connectionString);
            Levenshtein.RegisterLevenshteinFunction(connection);
            optionsBuilder.UseSqlite(connection);
        }, ServiceLifetime.Transient, ServiceLifetime.Transient);

        services.AddTransient<ItemSeeder>();

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

        _serviceProvider = services.BuildServiceProvider();
        _eventAggregator = Resolve<IEventAggregator>();
        SetupSqlite3();
    }

    protected override async Task LoadAsync()
    {
        var logger = Resolve<ILogger<Module>>();
        try
        {
            await FirstTimeSetup();
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "First-time setup failed.");
        }

        await using ChatLinksContext context = Resolve<ChatLinksContext>();
        await context.Database.MigrateAsync();

        _cornerIcon = Resolve<MainIcon>();
        _mainWindow = Resolve<MainWindow>();

        ItemSeeder seeder = Resolve<ItemSeeder>();
        await seeder.Seed(CancellationToken.None);

        _cornerIcon.Menu = new ContextMenuStrip();
        _syncButton = _cornerIcon.Menu.AddMenuItem("Sync database");
        _syncButton.Click += SyncClicked;
    }

    private static void SetupSqlite3()
    {
        SQLite3Provider_dynamic_cdecl.Setup("e_sqlite3", new ModuleGetFunctionPointer("sliekens.e_sqlite3"));
        raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
    }

    private async Task FirstTimeSetup()
    {
        var databaseLocation = DatabaseLocation();
        if (new FileInfo(databaseLocation) is { Exists: false } or { Length: 0 })
        {
            using var seed = ModuleParameters.ContentsManager.GetFileStream("data.zip");
            using var unzip = new ZipArchive(seed, ZipArchiveMode.Read);
            var data = unzip.GetEntry("data.db");
            if (data is not null)
            {
                using var dataStream = data.Open();
                using var fileStream = File.Create(databaseLocation);
                await dataStream.CopyToAsync(fileStream);
            }
        }
    }

    private T Resolve<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    private string DatabaseLocation()
    {
        string directory = ModuleParameters.DirectoriesManager.GetFullDirectoryPath("chat-links-data");
        return Path.Combine(directory, "data.db");
    }

    private async void SyncClicked(object sender, MouseEventArgs e)
    {
        var logger = Resolve<ILogger<Module>>();
        try
        {
            _syncButton!.Enabled = false;
            ItemSeeder seeder = Resolve<ItemSeeder>();
            if (Program.IsMainThread)
            {
                var seederTask = await Task.Factory.StartNew(async () =>
                {
                    await seeder.Seed(CancellationToken.None);
                }, TaskCreationOptions.LongRunning);
                await seederTask;
            }
            else
            {
                await seeder.Seed(CancellationToken.None);
            }

            ScreenNotification.ShowNotification("Everything is up-to-date.", ScreenNotification.NotificationType.Green);
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Sync failed");
            ScreenNotification.ShowNotification("Sync failed, try again later.",
                ScreenNotification.NotificationType.Warning);
        }
        finally
        {
            _syncButton!.Enabled = true;
        }
    }

    protected override void Unload()
    {
        _eventAggregator?.Publish(new ModuleUnloading());
        _cornerIcon?.Dispose();
        _mainWindow?.Dispose();
        _serviceProvider?.Dispose();
    }
}