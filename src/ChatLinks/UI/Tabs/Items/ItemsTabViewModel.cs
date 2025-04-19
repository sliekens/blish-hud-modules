using System.Collections.ObjectModel;

using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items;

public abstract class ContentArea(ContentArea? previous = null)
{
    protected ContentArea? Previous { get; } = previous;

    public abstract string GetTitle();

    public virtual AsyncTexture2D? GetIcon()
    {
        return Previous is not null
            ? AsyncTexture2D.FromAssetId(784268)
            : null;
    }

    public abstract ContentArea Search(string text);

    public virtual ContentArea Back()
    {
        return Previous ?? this;
    }

    public virtual ItemContentArea SelectItem()
    {
        return new ItemContentArea(this);
    }
}

public class ItemContentArea(ContentArea previous) : ContentArea(previous)
{
    private readonly ContentArea _previous = previous;

    public override string GetTitle()
    {
        return _previous.GetTitle();
    }

    public override ContentArea Search(string text)
    {
        return _previous.Search(text);
    }
}

public class RecentlyAddedContentArea : ContentArea
{
    public override string GetTitle()
    {
        return " ";
    }

    public override ContentArea Search(string text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? this
            : new SearchEverywhereContentArea(text, this);
    }
}

public class SearchEverywhereContentArea(string searchText, ContentArea previous) : ContentArea(previous)
{
    private readonly ContentArea _previous = previous;

    public override string GetTitle()
    {
        return searchText.Trim();
    }

    public override ContentArea Search(string text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? new RecentlyAddedContentArea()
            : new SearchEverywhereContentArea(text, _previous);
    }

    public override ContentArea Back()
    {
        return _previous;
    }
}

public class CategoryContentArea(string label) : ContentArea(new RecentlyAddedContentArea())
{
    public override string GetTitle()
    {
        return label;
    }

    public override ContentArea Search(string text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? this
            : new SearchCategoryContentArea(label, text, this);
    }
}

public class SearchCategoryContentArea(string label, string searchText, ContentArea previous) : ContentArea(previous)
{
    public override string GetTitle()
    {
        return $"{label}—{searchText.Trim()}";
    }

    public override ContentArea Search(string text)
    {
        ThrowHelper.ThrowIfNull(Previous);
        return string.IsNullOrWhiteSpace(text)
            ? Previous
            : new SearchCategoryContentArea(label, text, Previous);
    }
}

public sealed class ItemsTabViewModel(
    IStringLocalizer<ItemsTabView> localizer,
    IEventAggregator eventAggregator,
    IOptionsMonitor<ChatLinkOptions> options,
    ItemSearch search,
    ItemsListViewModel.Factory itemsListViewModelFactory,
    ChatLinkEditorViewModel.Factory chatLinkEditorViewModelFactory
) : ViewModel, IDisposable
{
    public delegate ItemsTabViewModel Factory();

    private ContentArea _area = new RecentlyAddedContentArea();

    public ContentArea Area
    {
        get => _area;
        private set
        {
            if (SetField(ref _area, value))
            {
                OnPropertyChanged(nameof(ContentIcon));
                OnPropertyChanged(nameof(ContentTitle));
            }
        }
    }

    private Item? _selectedItem;

    public Item? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }
    public ObservableCollection<ItemsListViewModel> SearchResults { get; } = [];

    public string SearchPlaceholder => localizer["Search placeholder"];

    private ObservableCollection<ItemCategoryMenuItem> _menuItems = [];

    public ObservableCollection<ItemCategoryMenuItem> MenuItems
    {
        get => _menuItems;
        private set => SetField(ref _menuItems, value);
    }

    private string _selectedCategory = "recently_added";

    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    private bool _searching;

    public bool Searching
    {
        get => _searching;
        set => SetField(ref _searching, value);
    }

    private int _searchNumber;

    private int _resultTotal;

    public int ResultTotal
    {
        get => _resultTotal;
        private set => SetField(ref _resultTotal, value);
    }

    private string _resultText = "";

    public string ResultText
    {
        get => _resultText;
        set => SetField(ref _resultText, value);
    }

    public AsyncTexture2D? ContentIcon => _area.GetIcon();

    public string ContentTitle => _area.GetTitle();

    public RelayCommand BackCommand => new(() =>
    {
        if (SelectedItem is not null)
        {
            SelectedItem = null;
            Area = Area.Back();
        }
        else if (!string.IsNullOrWhiteSpace(SearchText))
        {
            SearchText = "";
            Area = Area.Back();
        }
        else if (!string.IsNullOrWhiteSpace(SelectedCategory))
        {
            SelectedCategory = "recently_added";
            Area = Area.Back();
        }
    });

    public AsyncRelayCommand ShowRecentCommand => new(async () =>
    {
        SelectedItem = null;
        SearchText = "";
        SelectedCategory = "";
        Area = new RecentlyAddedContentArea();
        await NewItems(CancellationToken.None).ConfigureAwait(false);
    });

    public AsyncRelayCommand<ItemsFilter> ShowCategoryCommand => new(async filter =>
    {
        SelectedItem = null;
        SearchText = filter.Text ?? "";
        SelectedCategory = filter.Category ?? "";
        Area = new CategoryContentArea(filter.Label ?? "");
        await FilterItems(filter, CancellationToken.None).ConfigureAwait(false);
    });

    public RelayCommand<Item> SelectItemCommand => new(item =>
    {
        SelectedItem = item;
        Area = Area.SelectItem();
    });

    public AsyncRelayCommand SearchCommand => new(async () =>
    {
        await Task.Run(OnSearch).ConfigureAwait(false);
    });

    private List<ItemCategoryMenuItem> GetCategories()
    {
        return
        [
            new ItemCategoryMenuItem { Id = "recently_added", Label = localizer["Recently Added"] },
            new ItemCategoryMenuItem
            {
                Label = localizer["Armor"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "armor", Label = localizer["All Armor"] },
                    new ItemCategoryMenuItem { Id = "chest", Label = localizer["Chest"] },
                    new ItemCategoryMenuItem { Id = "leggings", Label = localizer["Leggings"] },
                    new ItemCategoryMenuItem { Id = "gloves", Label = localizer["Gloves"] },
                    new ItemCategoryMenuItem { Id = "helm", Label = localizer["Headgear"] },
                    new ItemCategoryMenuItem { Id = "helm_aquatic", Label = localizer["Aquatic Headgear"] },
                    new ItemCategoryMenuItem { Id = "boots", Label = localizer["Boots"] },
                    new ItemCategoryMenuItem { Id = "shoulders", Label = localizer["Shoulders"] }
                ]
            },
            new ItemCategoryMenuItem
            {
                Label = localizer["Weapons"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "weapon", Label = localizer["All Weapons"] },
                    new ItemCategoryMenuItem { Id = "axe", Label = localizer["Axes"] },
                    new ItemCategoryMenuItem { Id = "dagger", Label = localizer["Daggers"] },
                    new ItemCategoryMenuItem { Id = "focus", Label = localizer["Foci"] },
                    new ItemCategoryMenuItem { Id = "greatsword", Label = localizer["Greatswords"] },
                    new ItemCategoryMenuItem { Id = "hammer", Label = localizer["Hammers"] },
                    new ItemCategoryMenuItem { Id = "longbow", Label = localizer["Longbows"] },
                    new ItemCategoryMenuItem { Id = "sword", Label = localizer["Swords"] },
                    new ItemCategoryMenuItem { Id = "shortbow", Label = localizer["Short Bows"] },
                    new ItemCategoryMenuItem { Id = "mace", Label = localizer["Maces"] },
                    new ItemCategoryMenuItem { Id = "pistol", Label = localizer["Pistols"] },
                    new ItemCategoryMenuItem { Id = "rifle", Label = localizer["Rifles"] },
                    new ItemCategoryMenuItem { Id = "scepter", Label = localizer["Scepters"] },
                    new ItemCategoryMenuItem { Id = "staff", Label = localizer["Staffs"] },
                    new ItemCategoryMenuItem { Id = "torch", Label = localizer["Torches"] },
                    new ItemCategoryMenuItem { Id = "warhorn", Label = localizer["Warhorns"] },
                    new ItemCategoryMenuItem { Id = "shield", Label = localizer["Shields"] },
                    new ItemCategoryMenuItem { Id = "spear", Label = localizer["Spears"] },
                    new ItemCategoryMenuItem { Id = "harpoon_gun", Label = localizer["Harpoon Guns"] },
                    new ItemCategoryMenuItem { Id = "trident", Label = localizer["Tridents"] },
                    new ItemCategoryMenuItem { Id = "toy", Label = localizer["Toys"] },
                    new ItemCategoryMenuItem { Id = "toy_two_handed", Label = localizer["Toys (Two-Handed)"] },
                    new ItemCategoryMenuItem { Id = "small_bundle", Label = localizer["Small Bundles"] },
                    new ItemCategoryMenuItem { Id = "large_bundle", Label = localizer["Large Bundles"] }
                ]
            },

            new ItemCategoryMenuItem
            {
                Label = localizer["Upgrade Components"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "upgrade_component", Label = localizer["All Upgrade Components"] },
                    new ItemCategoryMenuItem { Id = "infusion", Label = localizer["Infusions"] },
                    new ItemCategoryMenuItem { Id = "enrichment", Label = localizer["Enrichments"] },
                    new ItemCategoryMenuItem { Id = "glyph", Label = localizer["Glyphs"] },
                    new ItemCategoryMenuItem { Id = "rune", Label = localizer["Runes"] },
                    new ItemCategoryMenuItem { Id = "rune_pvp", Label = localizer["Runes (PvP)"] },
                    new ItemCategoryMenuItem { Id = "sigil", Label = localizer["Sigils"] },
                    new ItemCategoryMenuItem { Id = "sigil_pvp", Label = localizer["Sigils (PvP)"] },
                    new ItemCategoryMenuItem { Id = "jewel", Label = localizer["Jewels"] },
                    new ItemCategoryMenuItem { Id = "universal_upgrade", Label = localizer["Universal Upgrades"] }
                ]
            },

            new ItemCategoryMenuItem
            {
                Label = localizer["Trinkets"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "trinket", Label = localizer["All Trinkets"] },
                    new ItemCategoryMenuItem { Id = "accessory", Label = localizer["Accessories"] },
                    new ItemCategoryMenuItem { Id = "amulet", Label = localizer["Amulets"] },
                    new ItemCategoryMenuItem { Id = "ring", Label = localizer["Rings"] }
                ]
            },
            new ItemCategoryMenuItem { Id = "back", Label = localizer["Back Items"] },
            new ItemCategoryMenuItem { Id = "relic", Label = localizer["Relics"] },
            new ItemCategoryMenuItem { Id = "power_core", Label = localizer["Power Cores"] },
            new ItemCategoryMenuItem { Id = "jade_tech_module", Label = localizer["Jade Tech Modules"] },
            new ItemCategoryMenuItem
            {
                Label = localizer["Gathering Tools"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "gathering_tool", Label = localizer["All Gathering Tools"] },

                    new ItemCategoryMenuItem { Id = "harvesting_sickle", Label = localizer["Harvesting Sickles"] },

                    new ItemCategoryMenuItem { Id = "logging_axe", Label = localizer["Logging Axes"] },
                    new ItemCategoryMenuItem { Id = "mining_pick", Label = localizer["Mining Picks"] },
                    new ItemCategoryMenuItem { Id = "fishing_rod", Label = localizer["Fishing Rods"] },
                    new ItemCategoryMenuItem { Id = "bait", Label = localizer["Bait"] },
                    new ItemCategoryMenuItem { Id = "lure", Label = localizer["Lures"] }
                ]
            },

            new ItemCategoryMenuItem
            {
                Label = localizer["Consumables"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "consumable", Label = localizer["All Consumables"] },
                    new ItemCategoryMenuItem { Id = "food", Label = localizer["Food"] },
                    new ItemCategoryMenuItem { Id = "utility", Label = localizer["Utilities"] },
                    new ItemCategoryMenuItem { Id = "booze", Label = localizer["Booze"] },
                    new ItemCategoryMenuItem { Id = "transmutation", Label = localizer["Transmutations"] },
                    new ItemCategoryMenuItem { Id = "upgrade_extractor", Label = localizer["Upgrade Extractors"] },
                    new ItemCategoryMenuItem { Id = "mount_license", Label = localizer["Mount Licenses"] },
                    new ItemCategoryMenuItem { Id = "unlocker", Label = localizer["Unlockers"] },
                    new ItemCategoryMenuItem { Id = "mount_skin_unlocker", Label = localizer["Mount Skin Unlockers"] },
                    new ItemCategoryMenuItem { Id = "outfit_unlocker", Label = localizer["Outfit Unlockers"] },
                    new ItemCategoryMenuItem
                    {
                        Id = "glider_skin_unlocker", Label = localizer["Glider Skin Unlockers"]
                    },

                    new ItemCategoryMenuItem
                    {
                        Id = "jade_bot_skin_unlocker", Label = localizer["Jade Bot Skin Unlockers"]
                    },

                    new ItemCategoryMenuItem { Id = "miniature_unlocker", Label = localizer["Miniature Unlockers"] },

                    new ItemCategoryMenuItem
                    {
                        Id = "mist_champion_skin_unlocker", Label = localizer["Mist Champion Skin Unlockers"]
                    },

                    new ItemCategoryMenuItem { Id = "dye", Label = localizer["Dyes"] },
                    new ItemCategoryMenuItem { Id = "recipe_sheet", Label = localizer["Recipe Sheets"] },
                    new ItemCategoryMenuItem { Id = "expansions", Label = localizer["Expansions"] },
                    new ItemCategoryMenuItem { Id = "content_unlocker", Label = localizer["Content Unlockers"] },
                    new ItemCategoryMenuItem { Id = "random_unlocker", Label = localizer["Random Unlocker"] },
                    new ItemCategoryMenuItem { Id = "appearance_changer", Label = localizer["Appearance Changers"] },
                    new ItemCategoryMenuItem { Id = "contract_npc", Label = localizer["Contract NPCs"] },
                    new ItemCategoryMenuItem { Id = "teleport_to_friend", Label = localizer["Teleport to Friend"] },
                    new ItemCategoryMenuItem
                    {
                        Id = "halloween_consumable", Label = localizer["Halloween Consumables"]
                    },
                    new ItemCategoryMenuItem { Id = "generic_consumable", Label = localizer["Generic Consumables"] }
                ]
            },

            new ItemCategoryMenuItem { Id = "currency", Label = localizer["Currencies"] },
            new ItemCategoryMenuItem { Id = "service", Label = localizer["Services"] },
            new ItemCategoryMenuItem
            {
                Label = localizer["Containers"],
                Subcategories =
                [
                    new ItemCategoryMenuItem { Id = "container", Label = localizer["All Containers"] },
                    new ItemCategoryMenuItem { Id = "default_container", Label = localizer["Normal Containers"] },
                    new ItemCategoryMenuItem { Id = "gift_box", Label = localizer["Gift Boxes"] },
                    new ItemCategoryMenuItem { Id = "immediate_container", Label = localizer["Immediate Containers"] },
                    new ItemCategoryMenuItem { Id = "black_lion_chest", Label = localizer["Black Lion Chests"] }
                ]
            },
            new ItemCategoryMenuItem { Id = "crafting_material", Label = localizer["Crafting Materials"] },
            new ItemCategoryMenuItem { Id = "gizmo", Label = localizer["Gizmos"] },
            new ItemCategoryMenuItem { Id = "miniature", Label = localizer["Miniatures"] },
            new ItemCategoryMenuItem { Id = "salvage_tool", Label = localizer["Salvage Tools"] },
            new ItemCategoryMenuItem { Id = "trophy", Label = localizer["Trophies"] }
        ];
    }

    public ChatLinkEditorViewModel CreateChatLinkEditorViewModel(Item item)
    {
        return chatLinkEditorViewModelFactory(item);
    }

    public Task Load()
    {
        MenuItems = [.. GetCategories()];
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Subscribe<DatabaseDownloaded>(OnDatabaseDownloaded);
        eventAggregator.Subscribe<DatabaseSeeded>(OnDatabaseSeeded);
        return Task.CompletedTask;
    }

    private async Task OnLocaleChanged(LocaleChanged args)
    {
        OnPropertyChanged(nameof(SearchPlaceholder));
        MenuItems = [.. GetCategories()];

        await Task.Run(OnSearch).ConfigureAwait(false);
    }

    private async Task OnDatabaseDownloaded(DatabaseDownloaded downloaded)
    {
        await Task.Run(OnSearch).ConfigureAwait(false);
    }

    private async Task OnDatabaseSeeded(DatabaseSeeded args)
    {
        if (args.Updated["items"] > 0)
        {
            await Task.Run(OnSearch).ConfigureAwait(false);
        }
    }

    private async Task OnSearch()
    {
        SelectedItem = null;
        string query = SearchText.Trim();
        switch (query.Length)
        {
            case 0:
                await FilterItems(new ItemsFilter
                {
                    Category = SelectedCategory
                }, CancellationToken.None).ConfigureAwait(false);
                break;
            case >= 3:
                await FilterItems(new ItemsFilter
                {
                    Category = SelectedCategory,
                    Text = query
                }, CancellationToken.None).ConfigureAwait(false);
                break;
            default:
                break;
        }

        Area = Area.Search(query);
    }

    private async Task NewItems(CancellationToken cancellationToken)
    {
        Searching = true;
        try
        {
            SearchResults.Clear();
            int maxResults = options.CurrentValue.MaxResultCount;
            await foreach (Item item in search.NewItems(maxResults).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ItemsListViewModel vm = itemsListViewModelFactory(item, item.Id == SelectedItem?.Id);
                SearchResults.Add(vm);
            }

            ResultTotal = await search.CountItems().ConfigureAwait(false);
            ResultText = ResultTotal <= maxResults
                ? localizer["Total results", ResultTotal]
                : localizer["Partial results", maxResults, ResultTotal];

        }
        finally
        {
            Searching = false;
        }
    }

    private async Task FilterItems(ItemsFilter filter, CancellationToken cancellationToken)
    {
        int searchNumber = Interlocked.Increment(ref _searchNumber);
        Searching = true;
        try
        {
            SearchResults.Clear();
            int maxResults = options.CurrentValue.MaxResultCount;
            ResultContext context = new();
            await foreach (Item item in search.FilterItems(filter, maxResults, context, cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested || searchNumber != _searchNumber)
                {
                    break;
                }

                ItemsListViewModel vm = itemsListViewModelFactory(item, item.Id == SelectedItem?.Id);
                SearchResults.Add(vm);
            }

            ResultTotal = context.ResultTotal;
            ResultText = ResultTotal <= maxResults
                ? localizer["Total results", ResultTotal]
                : localizer["Partial results", maxResults, ResultTotal];

        }
        finally
        {
            Searching = false;
        }
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Unsubscribe<DatabaseDownloaded>(OnDatabaseDownloaded);
        eventAggregator.Unsubscribe<DatabaseSeeded>(OnDatabaseSeeded);
    }
}
