using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Blish_HUD.Content;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.ChatLinks.UI.Tabs.Items2.Upgrades;
using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ChatLinkEditorViewModel : ViewModel
{
    private int _quantity = 1;

    private UpgradeComponent? _suffixItem;

    private UpgradeComponent? _secondarySuffixItem;

    private ItemLink _chatLinkBuilder;

    private readonly List<UpgradeEditorViewModel> _upgradeEditorViewModels;

    private readonly ItemTooltipViewModelFactory _tooltipViewModelFactory;

    private readonly UpgradeEditorViewModelFactory _upgradeEditorViewModelFactory;

    private readonly ItemIcons _icons;

    private readonly IClipBoard _clipboard;

    public ChatLinkEditorViewModel(
        ItemTooltipViewModelFactory tooltipViewModelFactory,
        UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
        ItemIcons icons,
        IClipBoard clipboard,
        Item item)
    {
        _tooltipViewModelFactory = tooltipViewModelFactory;
        _upgradeEditorViewModelFactory = upgradeEditorViewModelFactory;
        _icons = icons;
        _clipboard = clipboard;
        _chatLinkBuilder = new ItemLink { ItemId = item.Id };
        Item = item;
        ItemNameColor = ItemColors.Rarity(item.Rarity);
        _upgradeEditorViewModels = CreateUpgradeEditorViewModels().ToList();
        foreach (var (slot, vm) in _upgradeEditorViewModels.Select((vm, index) => (index + 1, vm)))
        {
            vm.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(vm.Customizing) when vm.Customizing:
                        foreach (var editor in UpgradeEditorViewModels)
                        {
                            editor.Customizing = editor == sender;
                        }

                        break;
                }
            };

            vm.UpgradeSlotViewModel.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case (nameof(vm.UpgradeSlotViewModel.SelectedUpgradeComponent)) when vm.UpgradeSlotViewModel.Type == UpgradeSlotType.Default:
                        switch (slot)
                        {
                            case 1:
                                SuffixItem = vm.UpgradeSlotViewModel.SelectedUpgradeComponent;
                                break;
                            case 2:
                                SecondarySuffixItem = vm.UpgradeSlotViewModel.SelectedUpgradeComponent;
                                break;
                        }

                        break;
                }
            };
        }
    }

    public Item Item { get; }

    public ObservableCollection<UpgradeEditorViewModel> UpgradeEditorViewModels
        => new(_upgradeEditorViewModels);

    public string ItemName
    {
        get
        {
            if (Quantity <= 1)
            {
                return Item.Name;
            }

            return $"{Quantity} {Item.Name}";
        }
    }

    public Color ItemNameColor { get; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            OnPropertyChanging(nameof(ItemName));
            OnPropertyChanging(nameof(ChatLink));
            SetField(ref _quantity, value);
            _chatLinkBuilder = _chatLinkBuilder with { Count = value };
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ChatLink));
        }
    }

    public UpgradeComponent? SuffixItem
    {
        get => _suffixItem;
        set
        {
            OnPropertyChanging(nameof(ItemName));
            OnPropertyChanging(nameof(ChatLink));
            SetField(ref _suffixItem, value);
            _chatLinkBuilder = _chatLinkBuilder with
            {
                SuffixItemId = value?.Id
            };
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ChatLink));
        }
    }

    public UpgradeComponent? SecondarySuffixItem
    {
        get => _secondarySuffixItem;
        set
        {
            OnPropertyChanging(nameof(ItemName));
            OnPropertyChanging(nameof(ChatLink));
            SetField(ref _secondarySuffixItem, value);
            _chatLinkBuilder = _chatLinkBuilder with
            {
                SecondarySuffixItemId = value?.Id
            };
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ChatLink));
        }
    }

    public string ChatLink
    {
        get => _chatLinkBuilder.ToString();
        [MemberNotNull(nameof(_chatLinkBuilder))]
        set
        {
            OnPropertyChanging();
            _chatLinkBuilder = ItemLink.Parse(value);
            OnPropertyChanged();
        }
    }

    public ICommand CopyCommand => new RelayCommand(OnCopy);

    public ICommand MinQuantityCommand => new RelayCommand(OnMinQuantity);

    public ICommand MaxQuantityCommand => new RelayCommand(OnMaxQuantity);

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return _tooltipViewModelFactory.Create(Item);
    }

    public AsyncTexture2D? GetIcon()
    {
        return _icons.GetIcon(Item);
    }

    private IEnumerable<UpgradeEditorViewModel> CreateUpgradeEditorViewModels()
    {
        if (Item is not IUpgradable upgradable)
        {
            yield break;
        }

        foreach (var defaultUpgradeComponentId in upgradable.UpgradeSlots)
        {
            yield return _upgradeEditorViewModelFactory.Create(
                Item,
                UpgradeSlotType.Default,
                defaultUpgradeComponentId
            );
        }

        foreach (var infusionSlot in upgradable.InfusionSlots)
        {
            yield return _upgradeEditorViewModelFactory.Create(
                Item,
                infusionSlot.Flags switch
                {
                    { Enrichment: true } => UpgradeSlotType.Enrichment,
                    { Infusion: true } => UpgradeSlotType.Infusion,
                    _ => UpgradeSlotType.Default
                },
                infusionSlot.ItemId
            );
        }
    }

    private void OnCopy()
    {
        _clipboard.SetText(ChatLink);
    }

    private void OnMinQuantity()
    {
        Quantity = 1;
    }

    private void OnMaxQuantity()
    {
        Quantity = 250;
    }
}