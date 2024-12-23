using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace SL.Common.Controls.Items.Upgrades;

public sealed class UpgradeSlot(ItemIcons icons) : Container
{
    public EventHandler<EventArgs>? Cleared;

    private FormattedLabel? _label;

    private UpgradeComponent? _upgradeComponent;

    private UpgradeComponent? _defaultUpgradeComponent;

    private UpgradeSlotType _slotType;

    public UpgradeComponent? UpgradeComponent
    {
        get => _upgradeComponent;
        set
        {
            _upgradeComponent = value;
            _label?.Dispose();
            _label = null;
            if (value is null)
            {
                Cleared?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public UpgradeComponent? DefaultUpgradeComponent
    {
        get => _defaultUpgradeComponent;
        set
        {
            _defaultUpgradeComponent = value;
            _label?.Dispose();
            _label = null;
        }
    }

    public UpgradeSlotType SlotType
    {
        get => _slotType;
        set
        {
            _slotType = value;
            _label?.Dispose();
            _label = null;
        }
    }

    public override void UpdateContainer(GameTime gameTime)
    {
        if (_label is null)
        {
            Format();
        }
    }

    protected override void DisposeControl()
    {
        _label?.Dispose();
        base.DisposeControl();
    }

    private void Format()
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .AutoSizeWidth()
            .AutoSizeHeight();

        if (UpgradeComponent is not null)
        {
            builder
                .CreatePart(" " + UpgradeComponent.Name, part =>
                {
                    part.SetPrefixImage(icons.GetIcon(UpgradeComponent));
                    part.SetPrefixImageSize(new Point(16));
                    part.SetHoverColor(Color.BurlyWood);
                    part.SetFontSize(ContentService.FontSize.Size16);
                });
        }
        else if (DefaultUpgradeComponent is not null)
        {
            builder
                .CreatePart(" " + DefaultUpgradeComponent.Name, part =>
                {
                    part.SetPrefixImage(icons.GetIcon(DefaultUpgradeComponent));
                    part.SetPrefixImageSize(new Point(16));
                    part.SetHoverColor(Color.BurlyWood);
                    part.SetFontSize(ContentService.FontSize.Size16);
                });
        }
        else
        {
            switch (SlotType)
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
        }

        _label = builder.Build();
        _label.Parent = this;

        if (UpgradeComponent is not null)
        {
            _label.Tooltip =
                new Tooltip(new ItemTooltipView(UpgradeComponent, icons, (Dictionary<int, UpgradeComponent>)[]));

            _label.Menu = new ContextMenuStrip();
            var remove = _label.Menu.AddMenuItem($"Remove {UpgradeComponent?.Name}");
            remove.Click += (_, _) =>
            {
                UpgradeComponent = null;
                OnCleared();
            };
        }
        else if (DefaultUpgradeComponent is not null)
        {
            _label.Tooltip =
                new Tooltip(new ItemTooltipView(DefaultUpgradeComponent, icons, (Dictionary<int, UpgradeComponent>)[]));
        }
        else
        {
            _label.BasicTooltipText = "Click to customize";
        }
    }

    private void OnCleared()
    {
        Cleared?.Invoke(this, EventArgs.Empty);
    }
}