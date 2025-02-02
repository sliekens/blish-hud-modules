﻿using System.ComponentModel;

using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSlot : Container
{
    public UpgradeSlotViewModel ViewModel { get; }

    private FormattedLabel _label;

    public UpgradeSlot(UpgradeSlotViewModel viewModel)
    {
        ViewModel = viewModel;
        ViewModel.PropertyChanged += PropertyChanged;
        _label = FormatSlot();
        _label.Parent = this;
    }

    private new void PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.SelectedUpgradeComponent):
            case nameof(ViewModel.DefaultUpgradeComponent):
            case nameof(ViewModel.Type):
                _label.Dispose();
                _label = FormatSlot();
                _label.Parent = this;
                break;
        }
    }

    public override void UpdateContainer(GameTime gameTime)
    {
        if (MouseOver)
        {
            if (ViewModel.SelectedUpgradeComponent is not null)
            {
                _label.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel(ViewModel.SelectedUpgradeComponent)));
            }
            else if (ViewModel.DefaultUpgradeComponent is not null)
            {
                _label.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel(ViewModel.DefaultUpgradeComponent)));
            }
            else
            {
                _label.BasicTooltipText ??=
                    """
                    Click to customize
                    Right-click for options
                    """;
            }
        }
    }

    private FormattedLabel FormatSlot()
    {
        return ViewModel switch
        {
            { SelectedUpgradeComponent: not null } => UsedSlot(ViewModel.SelectedUpgradeComponent),
            { DefaultUpgradeComponent: not null } => UsedSlot(ViewModel.DefaultUpgradeComponent),
            _ => UnusedSlot()
        };
    }

    private FormattedLabel UsedSlot(UpgradeComponent upgradeComponent)
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .AutoSizeWidth()
            .AutoSizeHeight()
            .CreatePart(" " + upgradeComponent.Name, part =>
            {
                part.SetPrefixImage(ViewModel.GetIcon(upgradeComponent));
                part.SetPrefixImageSize(new Point(16));
                part.SetHoverColor(Color.BurlyWood);
                part.SetFontSize(ContentService.FontSize.Size16);
            });

        return builder.Build();
    }

    private FormattedLabel UnusedSlot()
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
                        part.SetPrefixImage(EmbeddedResources.Texture("unused_upgrade_slot.png"));
                        part.SetPrefixImageSize(new Point(16));
                        part.SetFontSize(ContentService.FontSize.Size16);
                    });
                break;
            case UpgradeSlotType.Infusion:
                builder
                    .CreatePart(" Unused Infusion Slot", part =>
                    {
                        part.SetPrefixImage(Common.EmbeddedResources.Texture("unused_infusion_slot.png"));
                        part.SetPrefixImageSize(new Point(16));
                        part.SetFontSize(ContentService.FontSize.Size16);
                    });
                break;
            case UpgradeSlotType.Enrichment:
                builder
                    .CreatePart(" Unused Enrichment Slot", part =>
                    {
                        part.SetPrefixImage(Common.EmbeddedResources.Texture("unused_enrichment_slot.png"));
                        part.SetPrefixImageSize(new Point(16));
                        part.SetFontSize(ContentService.FontSize.Size16);
                    });
                break;
        }

        return builder.Build();
    }
}