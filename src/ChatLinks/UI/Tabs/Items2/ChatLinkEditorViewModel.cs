using System.Collections.ObjectModel;
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

using UpgradeSlot = SL.ChatLinks.UI.Tabs.Items2.Tooltips.UpgradeSlot;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ChatLinkEditorViewModel : ViewModel
{
    private bool _allowScroll = true;

    private int _quantity = 1;

    private UpgradeComponent? _suffixItem;

    private UpgradeComponent? _secondarySuffixItem;

    private readonly List<UpgradeEditorViewModel> _upgradeEditorViewModels;

    private readonly ItemTooltipViewModelFactory _tooltipViewModelFactory;

    private readonly UpgradeEditorViewModelFactory _upgradeEditorViewModelFactory;

    private readonly ItemIcons _icons;

    private readonly Customizer _customizer;

    private readonly IClipBoard _clipboard;

    public ChatLinkEditorViewModel(
        IEventAggregator eventAggregator,
        ItemTooltipViewModelFactory tooltipViewModelFactory,
        UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
        ItemIcons icons,
        Customizer customizer,
        IClipBoard clipboard,
        Item item)
    {
        _tooltipViewModelFactory = tooltipViewModelFactory;
        _upgradeEditorViewModelFactory = upgradeEditorViewModelFactory;
        _icons = icons;
        _customizer = customizer;
        _clipboard = clipboard;
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

        eventAggregator.Subscribe<MouseEnteredUpgradeSelector>(OnMouseEnteredUpgradeSelector);
        eventAggregator.Subscribe<MouseLeftUpgradeSelector>(OnMouseLeftUpgradeSelector);
    }
    private void OnMouseEnteredUpgradeSelector(MouseEnteredUpgradeSelector obj)
    {
        AllowScroll = false;
    }

    private void OnMouseLeftUpgradeSelector(MouseLeftUpgradeSelector obj)
    {
        AllowScroll = true;
    }

    public bool AllowScroll
    {
        get => _allowScroll;
        set => SetField(ref _allowScroll, value);
    }

    public Item Item { get; }

    public ObservableCollection<UpgradeEditorViewModel> UpgradeEditorViewModels
        => new(_upgradeEditorViewModels);

    public string ItemName
    {
        get
        {
            var name = Item.Name;

            if (!Item.Flags.HideSuffix)
            {
                var defaultSuffix = _customizer.DefaultSuffixItem(Item);
                if (!string.IsNullOrEmpty(defaultSuffix?.SuffixName) && name.EndsWith(defaultSuffix!.SuffixName))
                {
                    name = name[..^defaultSuffix.SuffixName.Length];
                    name = name.TrimEnd();
                }

                var newSuffix = SuffixName ?? defaultSuffix?.SuffixName;
                if (!string.IsNullOrEmpty(newSuffix))
                {
                    name += $" {newSuffix}";
                }
            }

            if (Quantity > 1)
            {
                name = $"{Quantity} {name}";
            }

            return name;
        }
    }

    public string? SuffixName => UpgradeEditorViewModels
        .FirstOrDefault(u => u is
        {
            UpgradeSlotViewModel:
            {
                Type: UpgradeSlotType.Default,
                SelectedUpgradeComponent: not null
            }
        })?.UpgradeSlotViewModel.SelectedUpgradeComponent?.SuffixName;

    public Color ItemNameColor { get; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            OnPropertyChanging(nameof(ItemName));
            OnPropertyChanging(nameof(ChatLink));
            SetField(ref _quantity, value);
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
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ChatLink));
        }
    }

    public string ChatLink
    {
        get => new ItemLink
        {
            ItemId = Item.Id,
            Count = Quantity,
            SuffixItemId = SuffixItem?.Id,
            SecondarySuffixItemId = SecondarySuffixItem?.Id
        }.ToString();

        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // Need a setter because this is bound to a writable TextBox
            // TODO: read-only TextBox with one-way binding and a Copy button
        }
    }

    public ICommand CopyCommand => new RelayCommand(OnCopy);

    public ICommand MinQuantityCommand => new RelayCommand(OnMinQuantity);

    public ICommand MaxQuantityCommand => new RelayCommand(OnMaxQuantity);

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        var upgrades = UpgradeEditorViewModels
            .Select(vm => new UpgradeSlot
            {
                Type = vm.UpgradeSlotType,
                UpgradeComponent = vm.EffectiveUpgradeComponent
            });
        return _tooltipViewModelFactory.Create(Item, Quantity, upgrades);
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