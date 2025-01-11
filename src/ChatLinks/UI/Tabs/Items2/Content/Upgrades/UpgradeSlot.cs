using Blish_HUD;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

using SL.Common;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeSlot : Container
{
    public UpgradeSlotViewModel ViewModel { get; }

    private FormattedLabel? _label;

    public UpgradeSlot(UpgradeSlotViewModel viewModel)
    {
        ViewModel = viewModel;
        Initialize();
    }

    private void Initialize()
    {
        _label = EmptySlot();
        _label.Parent = this;
        _label.BasicTooltipText =
            """
            Click to customize
            Right-click for options
            """;
    }

    private FormattedLabel EmptySlot()
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