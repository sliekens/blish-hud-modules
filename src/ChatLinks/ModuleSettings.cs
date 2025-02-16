using Blish_HUD.Settings;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using SL.ChatLinks.UI.Tabs.Items;

namespace SL.ChatLinks;

public class ModuleSettings : IConfigureOptions<ChatLinkOptions>, IOptionsChangeTokenSource<ChatLinkOptions>
{
    private readonly SettingEntry<bool> _bananaMode;

    private readonly SettingEntry<bool> _raiseStackSize;

    private readonly SettingEntry<int> _maxResultCount;

    private readonly ChangeTokenSource _changeTokenSource = new();

    public ModuleSettings(SettingCollection settings)
    {
        _bananaMode = settings.DefineSetting(
            "BananaMode",
            false,
            () => "Banana of Imagination-mode",
            () => "When enabled, you can add an upgrade component to any item."
        );

        _bananaMode.SettingChanged += (_, _) => _changeTokenSource.OnChange();

        _raiseStackSize = settings.DefineSetting(
            "RaiseStackSize",
            false,
            () => "Raise the maximum item stack size from 250 to 255",
            () => "When enabled, you can generate chat links with stacks of 255 items."
        );

        _raiseStackSize.SettingChanged += (_, _) => _changeTokenSource.OnChange();

        _maxResultCount = settings.DefineSetting(
            "MaxResultCount",
            50,
            () => "Maximum Result Count",
            () => "The maximum number of search results to display. WARNING! High numbers can slow down or even freeze Blish HUD."
        );

        _maxResultCount.SetRange(50, 1000);
        _maxResultCount.SettingChanged += (_, _) => _changeTokenSource.OnChange();
    }

    public bool BananaMode
    {
        get
        {
            return _bananaMode.Value;
        }
        set
        {
            _bananaMode.Value = value;
        }
    }

    public bool RaiseStackSize
    {
        get
        {
            return _raiseStackSize.Value;
        }
        set
        {
            _raiseStackSize.Value = value;
        }
    }

    public int MaxResultCount
    {
        get
        {
            return _maxResultCount.Value;
        }
        set
        {
            _maxResultCount.Value = value;
        }
    }

    public void Configure(ChatLinkOptions options)
    {
        options.RaiseStackSize = RaiseStackSize;
        options.BananaMode = BananaMode;
        options.MaxResultCount = MaxResultCount;
    }

    public IChangeToken GetChangeToken() => _changeTokenSource.Token;

    public string Name => Options.DefaultName;

    private class ChangeTokenSource
    {
        private CancellationTokenSource _cts = new();

        public IChangeToken Token => new CancellationChangeToken(_cts.Token);

        public void OnChange()
        {
            var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
            previousCts.Cancel();
            previousCts.Dispose();
        }
    }
}
