using System.Collections.Specialized;

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items.Controls;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items2;

public class ItemsTabView2 : View
{
    public ItemsTabViewModel ViewModel { get; }

    private readonly TextBox _searchBox;

    private ItemsList? _searchResults;

    public ItemsTabView2(ILogger<ItemsTabView> logger, ItemsTabViewModel viewModel)
    {
        ViewModel = viewModel;
        _searchBox = new TextBox
        {
            Width = 450,
            PlaceholderText = "Enter item name or chat link..."
        };
    }

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await ViewModel.LoadAsync();
        return true;
    }

    protected override void Build(Container buildPanel)
    {
        ViewModel.EnsureLoaded();
        _searchBox.Parent = buildPanel;

        _searchResults = new ItemsList(ViewModel.Upgrades)
        {
            Parent = buildPanel,
            Size = new Point(450, 500),
            Top = _searchBox.Bottom
        };

        foreach (var item in ViewModel.SearchResults)
        {
            _searchResults.AddOption(item);
        }

        ViewModel.SearchResults.CollectionChanged += SearchResultsChanged;

        Binder.Bind(ViewModel, vm => vm.SearchText, _searchBox);

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchEnterPressed;
        _searchBox.InputFocusChanged += SearchInputFocusChanged;
    }

    private void SearchResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_searchResults is null)
        {
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems)
                {
                    _searchResults.AddOption((Item)item);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (Item oldItem in e.OldItems)
                {
                    _searchResults.RemoveOption(oldItem);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (Item oldItem in e.OldItems)
                {
                    _searchResults.RemoveOption(oldItem);
                }
                foreach (Item newItem in e.NewItems)
                {
                    _searchResults.AddOption(newItem);
                }
                break;

            case NotifyCollectionChangedAction.Move:
                // Handle move if necessary
                break;

            case NotifyCollectionChangedAction.Reset:
                _searchResults.ClearOptions();
                foreach (Item item in ViewModel.SearchResults)
                {
                    _searchResults.AddOption(item);
                }
                break;
        }
    }

    protected override void Unload()
    {
        ViewModel.Unload();
        _searchBox.TextChanged -= SearchEnterPressed;
        _searchBox.EnterPressed -= SearchEnterPressed;
        _searchBox.InputFocusChanged -= SearchInputFocusChanged;
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        if (ViewModel.SearchCommand.CanExecute(null!))
        {
            ViewModel.SearchCommand.Execute(null!);
        }
    }

    private void SearchEnterPressed(object sender, EventArgs e)
    {
        if (ViewModel.SearchCommand.CanExecute(null!))
        {
            ViewModel.SearchCommand.Execute(null!);
        }
    }

    private void SearchInputFocusChanged(object sender, ValueEventArgs<bool> args)
    {
        if (args.Value)
        {
            _searchBox.SelectionStart = 0;
            _searchBox.SelectionEnd = _searchBox.Length;
        }
        else
        {
            _searchBox.SelectionStart = _searchBox.SelectionEnd;
        }
    }
}