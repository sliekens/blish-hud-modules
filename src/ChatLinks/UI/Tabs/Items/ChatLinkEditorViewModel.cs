using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

using Blish_HUD.Content;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common.ModelBinding;

using UpgradeSlot = SL.ChatLinks.UI.Tabs.Items.Tooltips.UpgradeSlot;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ChatLinkEditorViewModel : ViewModel, IDisposable
{
    private readonly IOptionsMonitor<ChatLinkOptions> _options;

    private readonly IStringLocalizer<ChatLinkEditor> _localizer;

    private readonly IEventAggregator _eventAggregator;

    private readonly IDbContextFactory _contextFactory;

    private Item _item;

    private UpgradeComponent? _suffixItem;

    private UpgradeComponent? _secondarySuffixItem;

    private bool _allowScroll = true;

    private int _quantity = 1;

    private readonly List<UpgradeEditorViewModel> _upgradeEditorViewModels;

    private readonly ItemTooltipViewModelFactory _tooltipViewModelFactory;

    private readonly UpgradeEditorViewModelFactory _upgradeEditorViewModelFactory;

    private readonly IconsService _icons;

    private readonly Customizer _customizer;

    private readonly IClipBoard _clipboard;

    private bool _showInfusionWarning;

    private int _maxStackSize;

    public ChatLinkEditorViewModel(
        IOptionsMonitor<ChatLinkOptions> options,
        IStringLocalizer<ChatLinkEditor> localizer,
        IEventAggregator eventAggregator,
        IDbContextFactory contextFactory,
        ItemTooltipViewModelFactory tooltipViewModelFactory,
        UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
        IconsService icons,
        Customizer customizer,
        IClipBoard clipboard,
        Item item)
    {
        ThrowHelper.ThrowIfNull(options);
        ThrowHelper.ThrowIfNull(eventAggregator);
        ThrowHelper.ThrowIfNull(item);
        _options = options;
        _localizer = localizer;
        _eventAggregator = eventAggregator;
        _contextFactory = contextFactory;
        _tooltipViewModelFactory = tooltipViewModelFactory;
        _upgradeEditorViewModelFactory = upgradeEditorViewModelFactory;
        _icons = icons;
        _customizer = customizer;
        _clipboard = clipboard;
        _item = item;
        ItemNameColor = ItemColors.Rarity(item.Rarity);
        _upgradeEditorViewModels = [.. CreateUpgradeEditorViewModels()];
        foreach ((int slot, UpgradeEditorViewModel vm) in _upgradeEditorViewModels.Select((vm, index) => (index + 1, vm)))
        {
            vm.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(vm.Customizing) when vm.Customizing:
                        foreach (UpgradeEditorViewModel editor in UpgradeEditorViewModels)
                        {
                            editor.Customizing = editor == sender;
                        }

                        break;
                    default:
                        break;
                }
            };

            vm.UpgradeSlotViewModel.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(vm.UpgradeSlotViewModel.SelectedUpgradeComponent) when vm.UpgradeSlotViewModel.Type == UpgradeSlotType.Default:
                        switch (slot)
                        {
                            case 1:
                                SuffixItem = vm.UpgradeSlotViewModel.SelectedUpgradeComponent;
                                break;
                            case 2:
                                SecondarySuffixItem = vm.UpgradeSlotViewModel.SelectedUpgradeComponent;
                                break;
                            default:
                                break;
                        }

                        break;
                    default:
                        break;
                }
            };
        }

        MaxStackSize = options.CurrentValue.RaiseStackSize ? 255 : 250;
        _ = options.OnChange(current =>
        {
            MaxStackSize = current.RaiseStackSize ? 255 : 250;
        });

        eventAggregator.Subscribe<MouseEnteredUpgradeSelector>(OnMouseEnteredUpgradeSelector);
        eventAggregator.Subscribe<MouseLeftUpgradeSelector>(OnMouseLeftUpgradeSelector);
        eventAggregator.Subscribe<UpgradeSlotChanged>(OnUpgradeSlotChanged);
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private async Task OnLocaleChanged(LocaleChanged args)
    {
        OnPropertyChanged(nameof(CopyNameLabel));
        OnPropertyChanged(nameof(CopyChatLinkLabel));
        OnPropertyChanged(nameof(OpenWikiLabel));
        OnPropertyChanged(nameof(OpenApiLabel));
        OnPropertyChanged(nameof(StackSizeLabel));
        OnPropertyChanged(nameof(ResetTooltip));
        OnPropertyChanged(nameof(InfusionWarning));

        ChatLinksContext context = _contextFactory.CreateDbContext(args.Language);
        await using (context.ConfigureAwait(false))
        {
            Item = context.Items.SingleOrDefault(item => item.Id == Item.Id);
        }
    }

    public int MaxStackSize
    {
        get => _maxStackSize;
        set => SetField(ref _maxStackSize, value);
    }

    private void OnUpgradeSlotChanged(UpgradeSlotChanged obj)
    {
        ShowInfusionWarning = UpgradeEditorViewModels.Where(editor => editor.UpgradeSlotType != UpgradeSlotType.Default)
            .Any(editor => editor.UpgradeSlotViewModel.SelectedUpgradeComponent is not null);
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

    public bool ShowInfusionWarning
    {
        get => _showInfusionWarning;
        private set => SetField(ref _showInfusionWarning, value);
    }

    public Item Item
    {
        get => _item;
        set
        {
            if (SetField(ref _item, value))
            {
                OnPropertyChanged(nameof(ItemName));
            }
        }
    }

    public ObservableCollection<UpgradeEditorViewModel> UpgradeEditorViewModels
        => [.. _upgradeEditorViewModels];

    public string ItemName
    {
        get
        {
            string name = Item.Name;

            if (!Item.Flags.HideSuffix)
            {
                UpgradeComponent? defaultSuffix = _customizer.DefaultSuffixItem(Item);
                if (!string.IsNullOrEmpty(defaultSuffix?.SuffixName) && name.EndsWith(defaultSuffix!.SuffixName, StringComparison.Ordinal))
                {
                    name = name[..^defaultSuffix.SuffixName.Length];
                    name = name.TrimEnd();
                }

                string? newSuffix = SuffixName ?? defaultSuffix?.SuffixName;
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
            _ = SetField(ref _quantity, value);
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
            _ = SetField(ref _suffixItem, value);
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
            _ = SetField(ref _secondarySuffixItem, value);
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ChatLink));
        }
    }

    public string ChatLink => new ItemLink
    {
        ItemId = Item.Id,
        Count = Quantity,
        SuffixItemId = SuffixItem?.Id,
        SecondarySuffixItemId = SecondarySuffixItem?.Id
    }.ToString();

    public string CopyNameLabel => _localizer["Copy Name"];

    public RelayCommand CopyNameCommand => new(() => _clipboard.SetText(Item.Name));

    public string CopyChatLinkLabel => _localizer["Copy Chat Link"];

    public RelayCommand CopyChatLinkCommand => new(() => _clipboard.SetText(ChatLink));

    public string OpenWikiLabel => _localizer["Open Wiki"];

    public RelayCommand OpenWikiCommand => new(() => Process.Start(_localizer["Wiki search", WebUtility.UrlEncode(Item.ChatLink)]));

    public string OpenApiLabel => _localizer["Open API"];

    public RelayCommand OpenApiCommand =>
        new(() => Process.Start(_localizer["Item API", Item.Id]));

    public RelayCommand MinQuantityCommand => new(() => Quantity = 1);

    public RelayCommand MaxQuantityCommand => new(() => Quantity = 250);

    public string StackSizeLabel => _localizer["Stack Size"];

    public string ResetTooltip => _localizer["Reset"];

    public string InfusionWarning => _localizer["Infusion warning"];

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        IEnumerable<UpgradeSlot> upgrades = UpgradeEditorViewModels
            .Select(vm => new UpgradeSlot
            {
                Type = vm.UpgradeSlotType,
                UpgradeComponent = vm.EffectiveUpgradeComponent
            });
        return _tooltipViewModelFactory.Create(Item, Quantity, upgrades);
    }

    public AsyncTexture2D? GetIcon()
    {
        return _icons.GetIcon(Item.IconHref);
    }

    private IEnumerable<UpgradeEditorViewModel> CreateUpgradeEditorViewModels()
    {
        if (Item is not IUpgradable upgradable)
        {
            if (_options.CurrentValue.BananaMode)
            {
                yield return _upgradeEditorViewModelFactory.Create(
                    Item,
                    UpgradeSlotType.Default,
                    null
                );
            }
            yield break;
        }

        foreach (int? defaultUpgradeComponentId in upgradable.UpgradeSlots)
        {
            yield return _upgradeEditorViewModelFactory.Create(
                Item,
                UpgradeSlotType.Default,
                _customizer.GetUpgradeComponent(defaultUpgradeComponentId)
            );
        }

        foreach (InfusionSlot infusionSlot in upgradable.InfusionSlots)
        {
            yield return _upgradeEditorViewModelFactory.Create(
                Item,
                infusionSlot.Flags switch
                {
                    { Enrichment: true } => UpgradeSlotType.Enrichment,
                    { Infusion: true } => UpgradeSlotType.Infusion,
                    _ => UpgradeSlotType.Default
                },
                _customizer.GetUpgradeComponent(infusionSlot.ItemId)
            );
        }
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<MouseEnteredUpgradeSelector>(OnMouseEnteredUpgradeSelector);
        _eventAggregator.Unsubscribe<MouseLeftUpgradeSelector>(OnMouseLeftUpgradeSelector);
        _eventAggregator.Unsubscribe<UpgradeSlotChanged>(OnUpgradeSlotChanged);
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}
