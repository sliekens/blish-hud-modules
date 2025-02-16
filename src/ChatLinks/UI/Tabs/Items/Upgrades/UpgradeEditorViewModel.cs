using System.Diagnostics;
using System.Net;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeEditorViewModel : ViewModel, IDisposable
{
    private bool _customizing;
    private readonly IStringLocalizer<UpgradeEditor> _localizer;
    private readonly IEventAggregator _eventAggregator;
    private readonly IClipBoard _clipboard;
    private readonly UpgradeSelectorViewModelFactory _upgradeComponentListViewModelFactory;

    public UpgradeEditorViewModel(
        IStringLocalizer<UpgradeEditor> localizer,
        IEventAggregator eventAggregator,
        IClipBoard clipboard,
        UpgradeSlotViewModel upgradeSlotViewModel,
        UpgradeSelectorViewModelFactory upgradeComponentListViewModelFactory,
        Item target)
    {
        _localizer = localizer;
        _eventAggregator = eventAggregator;
        _clipboard = clipboard;
        _upgradeComponentListViewModelFactory = upgradeComponentListViewModelFactory;
        UpgradeSlotViewModel = upgradeSlotViewModel;
        TargetItem = target;

        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);

        upgradeSlotViewModel.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(UpgradeSlotViewModel.DefaultUpgradeComponent):
                case nameof(UpgradeSlotViewModel.SelectedUpgradeComponent):
                    OnPropertyChanged(nameof(EffectiveUpgradeComponent));
                    break;
            }
        };
    }

    private void OnLocaleChanged(LocaleChanged changed)
    {
        OnPropertyChanged(nameof(CustomizeLabel));
        OnPropertyChanged(nameof(CancelLabel));
        OnPropertyChanged(nameof(CopyNameLabel));
        OnPropertyChanged(nameof(CopyChatLinkLabel));
        OnPropertyChanged(nameof(OpenWikiLabel));
        OnPropertyChanged(nameof(OpenApiLabel));
        OnPropertyChanged(nameof(RemoveItemLabel));
    }

    public bool Customizing
    {
        get => _customizing;
        set => SetField(ref _customizing, value);
    }

    public bool IsCustomizable => TargetItem is IUpgradable;

    public string CustomizeLabel => _localizer["Customize"];

    public string CancelLabel => _localizer["Cancel"];

    public RelayCommand CustomizeCommand => new(() => Customizing = !Customizing);

    public RelayCommand RemoveCommand => new(
        () =>
        {
            UpgradeSlotViewModel.SelectedUpgradeComponent = null;
            _eventAggregator.Publish(new UpgradeSlotChanged());
        },
        () => UpgradeSlotViewModel.SelectedUpgradeComponent is not null);

    public string CopyNameLabel => _localizer["Copy Name"];

    public RelayCommand CopyNameCommand => new(
        () =>
        {
            var name = EffectiveUpgradeComponent?.Name;
            if (name is not null)
            {
                _clipboard.SetText(name);
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public string CopyChatLinkLabel => _localizer["Copy Chat Link"];

    public RelayCommand CopyChatLinkCommand => new(
        () =>
        {
            var chatLink = EffectiveUpgradeComponent?.ChatLink;
            if (chatLink is not null)
            {
                _clipboard.SetText(chatLink);
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public string OpenWikiLabel => _localizer["Open Wiki"];

    public RelayCommand OpenWikiCommand => new(
        () =>
        {
            var chatLink = EffectiveUpgradeComponent?.ChatLink;
            if (chatLink is not null)
            {
                Process.Start(_localizer["Wiki search", WebUtility.UrlEncode(chatLink)]);
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public string OpenApiLabel => _localizer["Open API"];

    public RelayCommand OpenApiCommand => new(
        () =>
        {
            var id = EffectiveUpgradeComponent?.Id;
            if (id is not null)
            {
                Process.Start(_localizer["Item API", id]);
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public RelayCommand HideCommand => new(() => Customizing = false);

    public UpgradeSlotViewModel UpgradeSlotViewModel { get; }

    public Item TargetItem { get; }

    public UpgradeComponent? EffectiveUpgradeComponent =>
        UpgradeSlotViewModel.SelectedUpgradeComponent
        ?? UpgradeSlotViewModel.DefaultUpgradeComponent;

    public string RemoveItemLabel =>
        UpgradeSlotViewModel switch
        {
            { SelectedUpgradeComponent: not null } => _localizer["Remove upgrade", UpgradeSlotViewModel.SelectedUpgradeComponent.Name],
            { DefaultUpgradeComponent: not null } => _localizer["Remove upgrade", UpgradeSlotViewModel.DefaultUpgradeComponent.Name],
            _ => _localizer["Remove"]
        };

    public UpgradeSlotType UpgradeSlotType => UpgradeSlotViewModel.Type;

    public UpgradeSelectorViewModel CreateUpgradeComponentListViewModel()
    {
        UpgradeSelectorViewModel upgradeComponentListViewModel = _upgradeComponentListViewModelFactory.Create(
            TargetItem,
            UpgradeSlotViewModel.Type,
            UpgradeSlotViewModel.SelectedUpgradeComponent
        );

        upgradeComponentListViewModel.Selected += Selected;
        upgradeComponentListViewModel.Deselected += Deselected;

        return upgradeComponentListViewModel;
    }

    private void Selected(object sender, UpgradeComponent args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = args;
        Customizing = false;
        _eventAggregator.Publish(new UpgradeSlotChanged());
    }

    private void Deselected(object sender, EventArgs args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = null;
        Customizing = false;
        _eventAggregator.Publish(new UpgradeSlotChanged());
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}
