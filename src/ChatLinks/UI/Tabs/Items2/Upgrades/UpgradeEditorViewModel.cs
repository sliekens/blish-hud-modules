using System.Diagnostics;
using System.Net;

using GuildWars2.Items;

using SL.Common;
using SL.Common.Controls.Items.Upgrades;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeEditorViewModel(
    IEventAggregator eventAggregator,
    IClipBoard clipboard,
    UpgradeSlotViewModel upgradeSlotViewModel,
    UpgradeSelectorViewModelFactory upgradeComponentListViewModelFactory,
    Item target) : ViewModel
{
    private bool _customizing;

    public bool Customizing
    {
        get => _customizing;
        set => SetField(ref _customizing, value);
    }

    public RelayCommand CustomizeCommand => new(() => Customizing = !Customizing);

    public RelayCommand RemoveCommand => new(
        () =>
        {
            UpgradeSlotViewModel.SelectedUpgradeComponent = null;
            OnPropertyChanged(nameof(EffectiveUpgradeComponent));
            eventAggregator.Publish(new UpgradeSlotChanged());
        },
        () => UpgradeSlotViewModel.SelectedUpgradeComponent is not null);

    public RelayCommand CopyNameCommand => new(
        () =>
        {
            var name = EffectiveUpgradeComponent?.Name;
            if (name is not null)
            {
                clipboard.SetText(name);
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public RelayCommand CopyChatLinkCommand => new(
        () =>
        {
            var chatLink = EffectiveUpgradeComponent?.ChatLink;
            if (chatLink is not null)
            {
                clipboard.SetText(chatLink);
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public RelayCommand OpenWikiCommand => new(
        () =>
        {
            var chatLink = EffectiveUpgradeComponent?.ChatLink;
            if (chatLink is not null)
            {
                Process.Start($"https://wiki.guildwars2.com/wiki/?search={WebUtility.UrlEncode(chatLink)}");
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public RelayCommand OpenApiCommand => new(
        () =>
        {
            var id = EffectiveUpgradeComponent?.Id;
            if (id is not null)
            {
                Process.Start($"https://api.guildwars2.com/v2/items/{id}?v=latest");
            }
        },
        () => EffectiveUpgradeComponent is not null);

    public RelayCommand HideCommand => new(() => Customizing = false);

    public UpgradeSlotViewModel UpgradeSlotViewModel { get; } = upgradeSlotViewModel;

    public Item TargetItem { get; } = target;

    public UpgradeComponent? EffectiveUpgradeComponent =>
        UpgradeSlotViewModel.SelectedUpgradeComponent
        ?? UpgradeSlotViewModel.DefaultUpgradeComponent;

    public string RemoveItemText =>
        UpgradeSlotViewModel switch
        {
            { SelectedUpgradeComponent: not null } => $"Remove {UpgradeSlotViewModel.SelectedUpgradeComponent.Name}",
            { DefaultUpgradeComponent: not null } => $"Remove {UpgradeSlotViewModel.DefaultUpgradeComponent.Name}",
            _ => "Remove"
        };

    public UpgradeSlotType UpgradeSlotType => UpgradeSlotViewModel.Type;

    public UpgradeSelectorViewModel CreateUpgradeComponentListViewModel()
    {
        UpgradeSelectorViewModel upgradeComponentListViewModel = upgradeComponentListViewModelFactory.Create(
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
        OnPropertyChanged(nameof(EffectiveUpgradeComponent));
        eventAggregator.Publish(new UpgradeSlotChanged());
    }

    private void Deselected(object sender, EventArgs args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = null;
        Customizing = false;
        OnPropertyChanged(nameof(EffectiveUpgradeComponent));
        eventAggregator.Publish(new UpgradeSlotChanged());
    }
}