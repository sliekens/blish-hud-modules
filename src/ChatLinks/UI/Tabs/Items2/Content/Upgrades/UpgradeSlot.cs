using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Upgrades;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeSlot : Container
{
    public UpgradeSlotViewModel ViewModel { get; }

    private FormattedLabel _label;

    public UpgradeSlot(UpgradeSlotViewModel viewModel)
    {
        ViewModel = viewModel;
        Initialize();
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

    protected override void OnClick(MouseEventArgs e)
    {
        ViewModel.CustomizeCommand.Execute(null);
        base.OnClick(e);
    }

    [MemberNotNull(nameof(_label))]
    private void Initialize()
    {
        switch (ViewModel)
        {
            case { DefaultUpgradeComponent: not null }:
                _label = UsedSlot(ViewModel.DefaultUpgradeComponent);
                _label.Menu = new ContextMenuStrip(() => [
                    MenuItem("OnCustomizing", ViewModel.CustomizeCommand),
                ]);
                break;
            default:
                _label = UnusedSlot();
                _label.Menu = new ContextMenuStrip(() => [
                    MenuItem("OnCustomizing", ViewModel.CustomizeCommand),
                ]);
                break;
        }

        _label.Parent = this;
    }

    private ContextMenuStripItem MenuItem(string itemText, ICommand command)
    {
        var item = new ContextMenuStripItem(itemText);
        item.Click += (_, _) => command.Execute(null);
        return item;
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

        return builder.Build();
    }
}