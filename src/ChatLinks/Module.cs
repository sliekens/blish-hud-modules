using System.ComponentModel.Composition;
using System.IO.Compression;

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Integrations;
using SL.ChatLinks.Logging;
using SL.ChatLinks.Storage;
using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Items.Services;
using SL.ChatLinks.UI.Tabs.Items2;
using SL.ChatLinks.UI.Tabs.Items2.Collections;
using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.ChatLinks.UI.Tabs.Items2.Upgrades;
using SL.Common;
using SL.Common.Controls.Items.Services;

using SQLitePCL;

namespace SL.ChatLinks;

[Export(typeof(Blish_HUD.Modules.Module))]
[method: ImportingConstructor]
public class Module([Import("ModuleParameters")] ModuleParameters parameters) : Blish_HUD.Modules.Module(parameters)
{
    private MainIcon? _cornerIcon;

    private MainWindow? _mainWindow;

    private ServiceProvider? _serviceProvider;

    private ContextMenuStripItem? _syncButton;

    protected override void Initialize()
    {
        ServiceCollection services = new();
        services.AddSingleton(ModuleParameters);
        services.AddSingleton<IViewsFactory, ViewsFactory>();
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

        services.AddTransient<MainIcon>();
        services.AddTransient<MainIconViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ItemsTabView2>();
        services.AddTransient<ItemsTabViewModel>();
        services.AddTransient<ItemsListViewModel>();
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

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ServiceLocator.ServiceProvider = serviceProvider;
        _serviceProvider = serviceProvider;

        SetupSQLite3();
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
        _cornerIcon.Click += CornerIcon_Click;

        ItemSeeder seeder = Resolve<ItemSeeder>();
        Progress<string> progress = new(report =>
        {
            _cornerIcon.LoadingMessage = report;
        });

        if (Program.IsMainThread)
        {
            var seederTask = await Task.Factory.StartNew(async () =>
            {
                await seeder.Seed(progress, CancellationToken.None);
            }, TaskCreationOptions.LongRunning);
            await seederTask;
        }
        else
        {
            await seeder.Seed(progress, CancellationToken.None);
        }

        _cornerIcon.LoadingMessage = null;
        _cornerIcon.BasicTooltipText = null;
        _cornerIcon.Menu = new ContextMenuStrip();
        _syncButton = _cornerIcon.Menu.AddMenuItem("Sync database");
        _syncButton.Click += SyncClicked;
    }

    private void SetupSQLite3()
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
            Progress<string> progress = new(report =>
            {
                _cornerIcon!.LoadingMessage = report;
            });

            if (Program.IsMainThread)
            {
                var seederTask = await Task.Factory.StartNew(async () =>
                {
                    await seeder.Seed(progress, CancellationToken.None);
                }, TaskCreationOptions.LongRunning);
                await seederTask;
            }
            else
            {
                await seeder.Seed(progress, CancellationToken.None);
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
            _cornerIcon!.LoadingMessage = null;
            _cornerIcon!.BasicTooltipText = null;
            _syncButton!.Enabled = true;
        }
    }

    private void CornerIcon_Click(object sender, EventArgs e)
    {
        _mainWindow?.ToggleWindow();
    }

    protected override void Unload()
    {
        _cornerIcon?.Dispose();
        _mainWindow?.Dispose();
        _serviceProvider?.Dispose();
        ServiceLocator.ServiceProvider = null;
    }
}