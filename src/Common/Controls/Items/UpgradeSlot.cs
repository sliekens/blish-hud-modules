using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace SL.Common.Controls.Items;

public sealed class UpgradeSlot(ItemIcons icons) : Container
{
    private FormattedLabel? _label;

    private UpgradeComponent? _upgradeComponent;

    public UpgradeComponent? UpgradeComponent
    {
        get
        {
            return _upgradeComponent;
        }
        set
        {
            _upgradeComponent = value;
            _label?.Dispose();
            _label = null;
        }
    }

    public override void UpdateContainer(GameTime gameTime)
    {
        if (_label is null)
        {
            Format(_upgradeComponent);
        }

        base.UpdateContainer(gameTime);
    }

    private void Format(UpgradeComponent? component)
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .AutoSizeWidth()
            .AutoSizeHeight();

        if (_upgradeComponent is not null)
        {
            builder
                .CreatePart(" " + _upgradeComponent.Name, part =>
                {
                    part.SetPrefixImage(icons.GetIcon(_upgradeComponent));
                    part.SetPrefixImageSize(new Point(16));
                    part.SetHoverColor(Color.BurlyWood);
                    part.SetFontSize(ContentService.FontSize.Size16);
                });
        }
        else
        {
            builder
                .CreatePart(" Unused Upgrade Slot", part =>
                {
                    part.SetPrefixImage(Resources.Texture("unused_upgrade_slot.png"));
                    part.SetPrefixImageSize(new Point(16));
                    part.SetFontSize(ContentService.FontSize.Size16);
                });
        }

        _label = builder.Build();
        _label.Parent = this;

        if (UpgradeComponent is not null)
        {
            _label.Tooltip = new Tooltip(new ItemTooltipView(UpgradeComponent, icons, (Dictionary<int, UpgradeComponent>)[]));
        }
    }

    protected override void DisposeControl()
    {
        _label?.Dispose();
        base.DisposeControl();
    }
}