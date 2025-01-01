using System.ComponentModel;
using System.Diagnostics;
using System.Net;

using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace SL.Common.Controls.Items.Upgrades;

public sealed class UpgradeSlot : Container
{
    public UpgradeSlotViewModel ViewModel { get; }

    private FormattedLabel? _label;

    public UpgradeSlot(UpgradeSlotViewModel vm)
    {
        ViewModel = vm;
        ViewModel.PropertyChanged += OnChanges;
        Click += (_, _) =>
        {
            vm.OnCustomize();
        };

        UpdateSlot();
    }

    private void UpdateSlot()
    {
        if (ViewModel.SelectedUpgradeComponent is not null)
        {
            FillSlot(new UsedUpgradeSlot
            {
                UpgradeComponent = ViewModel.SelectedUpgradeComponent,
                Icon = ViewModel.Icons.GetIcon(ViewModel.SelectedUpgradeComponent),
                IsDefault = false
            });
        }
        else if (ViewModel.DefaultUpgradeComponent is not null)
        {
            FillSlot(new UsedUpgradeSlot
            {
                UpgradeComponent = ViewModel.DefaultUpgradeComponent,
                Icon = ViewModel.Icons.GetIcon(ViewModel.DefaultUpgradeComponent),
                IsDefault = true
            });
        }
        else
        {
            ClearSlot();
        }
    }

    private void OnChanges(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UpgradeSlotViewModel.SelectedUpgradeComponent))
        {
            UpdateSlot();
        }
    }

    protected override void DisposeControl()
    {
        _label?.Dispose();
        base.DisposeControl();
    }

    private void FillSlot(UsedUpgradeSlot slot)
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .AutoSizeWidth()
            .AutoSizeHeight()
            .CreatePart(" " + slot.UpgradeComponent.Name, part =>
            {
                part.SetPrefixImage(slot.Icon);
                part.SetPrefixImageSize(new Point(16));
                part.SetHoverColor(Color.BurlyWood);
                part.SetFontSize(ContentService.FontSize.Size16);
            });

        _label?.Dispose();
        _label = builder.Build();
        _label.Parent = this;
        _label.Menu = Context(slot);
        _label.MouseEntered += (_, _) =>
        {
            _label.Tooltip ??=
                new Tooltip(new ItemTooltipView(slot.UpgradeComponent, (Dictionary<int, UpgradeComponent>)[]));
        };
    }

    private void ClearSlot()
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .AutoSizeWidth()
            .AutoSizeHeight();

        switch (ViewModel.Type)
        {
            case UpgradeSlotType.Default:
                builder
                    .CreatePart(" Unused Upgrade Slot", part =>
                    {
                        part.SetPrefixImage(Resources.Texture("unused_upgrade_slot.png"));
                        part.SetPrefixImageSize(new Point(16));
                        part.SetFontSize(ContentService.FontSize.Size16);
                    });
                break;
            case UpgradeSlotType.Infusion:
                builder
                    .CreatePart(" Unused Infusion Slot", part =>
                    {
                        part.SetPrefixImage(Resources.Texture("unused_infusion_slot.png"));
                        part.SetPrefixImageSize(new Point(16));
                        part.SetFontSize(ContentService.FontSize.Size16);
                    });
                break;
            case UpgradeSlotType.Enrichment:
                builder
                    .CreatePart(" Unused Enrichment Slot", part =>
                    {
                        part.SetPrefixImage(Resources.Texture("unused_enrichment_slot.png"));
                        part.SetPrefixImageSize(new Point(16));
                        part.SetFontSize(ContentService.FontSize.Size16);
                    });
                break;
        }

        _label?.Dispose();
        _label = builder.Build();
        _label.Parent = this;
        _label.BasicTooltipText = """
            Click to customize
            Right-click for options
            """;
        _label.Menu = Context(null);
    }

    private ContextMenuStrip Context(UsedUpgradeSlot? slot)
    {
        var menu = new ContextMenuStrip();
        var customize = menu.AddMenuItem("Customize");
        customize.Click += (_, _) =>
        {
            ViewModel.OnCustomize();
        };

        if (slot is null)
        {
            return menu;
        }

        UpgradeComponent item = slot.UpgradeComponent;
        if (!slot.IsDefault)
        {
            var remove = menu.AddMenuItem($"Remove {item.Name}");
            remove.Click += (_, _) =>
            {
                ViewModel.OnClear();
            };
        }

        ContextMenuStripItem copyName = menu.AddMenuItem("Copy Name");
        copyName.Click += async (_, _) => await ClipboardUtil.WindowsClipboardService.SetTextAsync(item.Name);

        ContextMenuStripItem copyLink = menu.AddMenuItem("Copy Chat Link");
        copyLink.Click += async (_, _) => await ClipboardUtil.WindowsClipboardService.SetTextAsync(item.ChatLink);

        ContextMenuStripItem wiki = menu.AddMenuItem("Wiki");
        wiki.Click += (_, _) => Process.Start($"https://wiki.guildwars2.com/wiki/?search={WebUtility.UrlEncode(item.ChatLink)}");

        ContextMenuStripItem api = menu.AddMenuItem("API");
        api.Click += (_, _) => Process.Start($"https://api.guildwars2.com/v2/items/{item.Id}?v=latest");

        return menu;
    }
}