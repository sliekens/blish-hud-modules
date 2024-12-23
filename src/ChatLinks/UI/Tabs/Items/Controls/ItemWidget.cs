using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using SL.Common.Controls;
using SL.Common.Controls.Items;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemWidget : FlowPanel
{
    private readonly TextBox _chatLink;

    private readonly Item _item;

    private readonly ItemIcons _icons;

    private readonly IDictionary<int, UpgradeComponent> _upgrades;

    private readonly ItemImage _itemIcon;

    private readonly ItemName _itemName;

    private readonly NumberPicker _numberPicker;

    private readonly UpgradeSlot? _upgradeSlot1;

    private readonly UpgradeSlot? _upgradeSlot2;

    private readonly UpgradeSlot? _infusionSlot1;

    private readonly UpgradeSlot? _infusionSlot2;

    private readonly UpgradeSlot? _infusionSlot3;

    private readonly Label? _infusionWarning;

    private readonly UpgradeComponentsList? _upgradeComponentList1;

    private readonly UpgradeComponentsList? _upgradeComponentList2;

    private readonly UpgradeComponentsList? _infusionList1;

    private readonly UpgradeComponentsList? _infusionList2;

    private readonly UpgradeComponentsList? _infusionList3;

    public ItemWidget(Item item, IDictionary<int, UpgradeComponent> upgrades, ItemIcons icons)
    {
        ShowTint = true;
        ShowBorder = true;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(20);
        OuterControlPadding = new Vector2(10f);
        AutoSizePadding = new Point(10);
        Width = 350;
        HeightSizingMode = SizingMode.Fill;
        ContentRegion = new Rectangle(5, 5, 290, 520);
        CanScroll = true;

        _item = item;
        _icons = icons;
        _upgrades = upgrades;

        var header = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            ControlPadding = new Vector2(5f),
            Width = Width - 5,
            Height = 50,
            Parent = this
        };

        _itemIcon = new ItemImage(item, icons)
        {
            Parent = header,
            Tooltip = new Tooltip(new ItemTooltipView(item, icons, upgrades))
        };

        header.Menu = new ItemContextMenu(item);

        _itemName = new ItemName(item)
        {
            Parent = header,
            Width = header.Width - 50,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Middle,
            Font = GameService.Content.DefaultFont18,
            WrapText = true
        };

        _itemName.Text = _itemName.Text.Replace(" ", "  ");

        if (!item.Flags.NotUpgradeable)
        {
            _upgradeSlot1 = CreateUpgradeSlot(UpgradeSlotType.Default, _item switch
            {
                Armor { SuffixItemId: not null } armor =>
                    _upgrades.TryGetValue(armor.SuffixItemId.Value, out var upgradeComponent) ? upgradeComponent : null,
                Backpack { SuffixItemId: not null } back =>
                    _upgrades.TryGetValue(back.SuffixItemId.Value, out var upgradeComponent) ? upgradeComponent : null,
                Trinket { SuffixItemId: not null } trinket =>
                    _upgrades.TryGetValue(trinket.SuffixItemId.Value, out var upgradeComponent) ? upgradeComponent : null,
                Weapon { SuffixItemId: not null } weapon =>
                    _upgrades.TryGetValue(weapon.SuffixItemId.Value, out var upgradeComponent) ? upgradeComponent : null,
                _ => null
            });

            _upgradeComponentList1 = CreateUpgradeComponentsList(UpgradeSlotType.Default);
            _upgradeSlot2 = CreateUpgradeSlot(UpgradeSlotType.Default, _item switch
            {
                Weapon { SecondarySuffixItemId: not null } weapon =>
                    _upgrades.TryGetValue(weapon.SecondarySuffixItemId.Value, out var upgradeComponent) ? upgradeComponent : null,
                _ => null
            });

            _upgradeComponentList2 = CreateUpgradeComponentsList(UpgradeSlotType.Default);
            _upgradeSlot1.Click += UpgradeSlot1Clicked;
            _upgradeSlot2.Click += UpgradeSlot2Clicked;
            _upgradeSlot1.Cleared += UpgradeSlotCleared;
            _upgradeSlot2.Cleared += UpgradeSlotCleared;
            _upgradeComponentList1.UpgradeComponentSelected += UpgradeComponent1Selected;
            _upgradeComponentList2.UpgradeComponentSelected += UpgradeComponent2Selected;
        }

        IEnumerable<InfusionSlot> infusionSlots = item switch
        {
            Armor armor => armor.InfusionSlots,
            Weapon weapon => weapon.InfusionSlots,
            Backpack back => back.InfusionSlots,
            Trinket trinket => trinket.InfusionSlots,
            _ => []
        };

        foreach ((InfusionSlot? infusionSlot, int index) in infusionSlots.Select((entry, index) => (entry, index)))
        {
            var slot = CreateInfusionSlot(infusionSlot);
            if (slot is null)
            {
                continue;
            }

            var list = CreateInfusionList(infusionSlot);
            if (list is null)
            {
                continue;
            }

            switch (index)
            {
                case 0:
                    _infusionSlot1 = slot;
                    _infusionList1 = list;
                    slot.Click += InfusionSlot1Clicked;
                    slot.Cleared += UpgradeSlotCleared;
                    list.UpgradeComponentSelected += InfusionSlot1Selected;
                    break;
                case 1:
                    _infusionSlot2 = slot;
                    _infusionList2 = list;
                    slot.Click += InfusionSlot2Clicked;
                    slot.Cleared += UpgradeSlotCleared;
                    list.UpgradeComponentSelected += InfusionSlot2Selected;
                    break;
                case 2:
                    _infusionSlot3 = slot;
                    _infusionList3 = list;
                    slot.Click += InfusionSlot3Clicked;
                    slot.Cleared += UpgradeSlotCleared;
                    list.UpgradeComponentSelected += InfusionSlot3Selected;
                    break;
            }
        }

        _infusionWarning = new Label
        {
            Parent = this,
            Width = Width - 20,
            AutoSizeHeight = true,
            WrapText = true,
            TextColor = Color.Yellow,
            Text = "Due to technical restrictions, the chat link will show the item's default infusions instead of the selected infusion(s)."
        };

        var quantityGroup = new FlowPanel
        {
            Parent = this,
            FlowDirection = ControlFlowDirection.LeftToRight,
            Width = Width,
            HeightSizingMode = SizingMode.AutoSize
        };

        Label quantityLabel = new()
        {
            Parent = quantityGroup,
            Text = "Quantity:",
            AutoSizeWidth = true,
            AutoSizeHeight = true
        };

        _numberPicker = new NumberPicker
        {
            Parent = quantityGroup,
            Width = 80,
            Value = 1,
            MinValue = 1,
            MaxValue = 250
        };

        StandardButton minQuantity = new StandardButton
        {
            Parent = quantityGroup,
            Text = "Min",
            Width = 40,
            Height = Height,
        };

        StandardButton maxQuantity = new StandardButton
        {
            Parent = quantityGroup,
            Text = "Max",
            Width = 40,
            Height = Height,
        };

        Label chatLinkLabel = new() { Parent = this, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox
        {
            Parent = this,
            Text = item.ChatLink,
            Width = 150
        };

        _itemIcon.Click += HeaderClicked;
        _numberPicker.TextChanged += NumberPickerChanged;
        _chatLink.Click += ChatLinkClicked;
        minQuantity.Click += (_, _) => _numberPicker.Value = 1;
        maxQuantity.Click += (_, _) => _numberPicker.Value = 250;
    }

    public override void UpdateContainer(GameTime gameTime)
    {
        if (_infusionWarning is not null)
        {
            _infusionWarning.Visible = _infusionSlot1?.UpgradeComponent is not null
                || _infusionSlot2?.UpgradeComponent is not null
                || _infusionSlot3?.UpgradeComponent is not null;
            Invalidate();
        }

        base.UpdateContainer(gameTime);
    }

    private void UpgradeSlot1Clicked(object sender, MouseEventArgs e)
    {
        if (_upgradeComponentList1 is { Visible: true })
        {
            _upgradeComponentList1.Hide();
        }
        else
        {
            _upgradeComponentList1?.Show();
        }

        Invalidate();
    }

    private void UpgradeSlot2Clicked(object sender, MouseEventArgs e)
    {
        if (_upgradeComponentList2 is { Visible: true })
        {
            _upgradeComponentList2.Hide();
        }
        else
        {
            _upgradeComponentList2?.Show();
        }

        Invalidate();
    }

    private void InfusionSlot1Clicked(object sender, MouseEventArgs e)
    {
        if (_infusionList1 is { Visible: true })
        {
            _infusionList1.Hide();
        }
        else
        {
            _infusionList1?.Show();
        }

        Invalidate();
    }

    private void InfusionSlot2Clicked(object sender, MouseEventArgs e)
    {
        if (_infusionList2 is { Visible: true })
        {
            _infusionList2.Hide();
        }
        else
        {
            _infusionList2?.Show();
        }

        Invalidate();
    }

    private void InfusionSlot3Clicked(object sender, MouseEventArgs e)
    {
        if (_infusionList3 is { Visible: true })
        {
            _infusionList3.Hide();
        }
        else
        {
            _infusionList3?.Show();
        }

        Invalidate();
    }

    private void UpgradeComponent1Selected(object sender, UpgradeComponentSelectedArgs e)
    {
        if (_upgradeSlot1 is null)
        {
            return;
        }

        if (_upgradeSlot1.UpgradeComponent != e.Selected)
        {
            _upgradeSlot1.UpgradeComponent = e.Selected;
            UpdateTooltip();
            UpdateChatLink();
        }
        else
        {
            _upgradeSlot1.UpgradeComponent = null;
        }

        _upgradeComponentList1?.Hide();
        Invalidate();
    }

    private void UpgradeComponent2Selected(object sender, UpgradeComponentSelectedArgs e)
    {
        if (_upgradeSlot2 is null)
        {
            return;
        }

        if (_upgradeSlot2.UpgradeComponent != e.Selected)
        {
            _upgradeSlot2.UpgradeComponent = e.Selected;
            UpdateTooltip();
            UpdateChatLink();
        }
        else
        {
            _upgradeSlot2.UpgradeComponent = null;
        }

        _upgradeComponentList2?.Hide();
        Invalidate();
    }

    private void InfusionSlot1Selected(object sender, UpgradeComponentSelectedArgs e)
    {
        if (_infusionSlot1 is null)
        {
            return;
        }

        if (_infusionSlot1.UpgradeComponent != e.Selected)
        {
            _infusionSlot1.UpgradeComponent = e.Selected;
            UpdateTooltip();
            UpdateChatLink();
        }
        else
        {
            _infusionSlot1.UpgradeComponent = null;
        }

        _infusionList1?.Hide();
        Invalidate();
    }

    private void InfusionSlot2Selected(object sender, UpgradeComponentSelectedArgs e)
    {
        if (_infusionSlot2 is null)
        {
            return;
        }

        if (_infusionSlot2.UpgradeComponent != e.Selected)
        {
            _infusionSlot2.UpgradeComponent = e.Selected;
            UpdateTooltip();
            UpdateChatLink();
        }
        else
        {
            _infusionSlot2.UpgradeComponent = null;
        }

        _infusionList2?.Hide();
        Invalidate();
    }

    private void InfusionSlot3Selected(object sender, UpgradeComponentSelectedArgs e)
    {
        if (_infusionSlot3 is null)
        {
            return;
        }

        if (_infusionSlot3.UpgradeComponent != e.Selected)
        {
            _infusionSlot3.UpgradeComponent = e.Selected;
            UpdateTooltip();
            UpdateChatLink();
        }
        else
        {
            _infusionSlot3.UpgradeComponent = null;
        }

        _infusionList3?.Hide();
        Invalidate();
    }

    private void UpgradeSlotCleared(object sender, EventArgs e)
    {
        UpdateChatLink();
        UpdateTooltip();
    }

    protected override void OnMouseWheelScrolled(MouseEventArgs e)
    {
        if (_upgradeComponentList1?.MouseOver == true)
        {
            return;
        }

        if (_upgradeComponentList2?.MouseOver == true)
        {
            return;
        }

        if (_infusionList1?.MouseOver == true)
        {
            return;
        }

        if (_infusionList2?.MouseOver == true)
        {
            return;
        }

        if (_infusionList3?.MouseOver == true)
        {
            return;
        }

        base.OnMouseWheelScrolled(e);
    }

    private void NumberPickerChanged(object sender, EventArgs e)
    {
        _itemName.Quantity = _numberPicker.Value;
        UpdateChatLink();
    }

    private void HeaderClicked(object sender, MouseEventArgs e)
    {
        switch (GameService.Input.Keyboard.ActiveModifiers)
        {
            case ModifierKeys.Ctrl:
                GameService.GameIntegration.Chat.Send(_chatLink.Text);
                break;

            // Shift interferes with ability to activate chat
            case not ModifierKeys.Shift:
                GameService.GameIntegration.Chat.Paste(_chatLink.Text);
                break;
        }
    }

    private void ChatLinkClicked(object sender, MouseEventArgs e)
    {
        _chatLink.SelectionStart = 0;
        _chatLink.SelectionEnd = _chatLink.Text.Length;
    }

    private void UpdateTooltip()
    {
        _itemIcon.Tooltip = new Tooltip(new ItemTooltipView(_item switch
        {
            Armor armor => armor with
            {
                SuffixItemId = _upgradeSlot1?.UpgradeComponent?.Id ?? _upgradeSlot1?.DefaultUpgradeComponent?.Id,
                InfusionSlots = GetSelectedInfusionSlots().ToList()
            },
            Weapon weapon => weapon with
            {
                SuffixItemId = _upgradeSlot1?.UpgradeComponent?.Id ?? _upgradeSlot1?.DefaultUpgradeComponent?.Id,
                SecondarySuffixItemId = _upgradeSlot2?.UpgradeComponent?.Id ?? _upgradeSlot2?.DefaultUpgradeComponent?.Id,
                InfusionSlots = GetSelectedInfusionSlots().ToList()
            },
            Backpack back => back with
            {
                SuffixItemId = _upgradeSlot1?.UpgradeComponent?.Id,
                InfusionSlots = GetSelectedInfusionSlots().ToList()
            },
            Trinket trinket => trinket with
            {
                SuffixItemId = _upgradeSlot1?.UpgradeComponent?.Id,
                InfusionSlots = GetSelectedInfusionSlots().ToList()
            },
            _ => _item
        }, _icons, _upgrades));
    }

    private IEnumerable<InfusionSlot> GetInfusionSlots()
    {
        return _item switch
        {
            Armor armor => armor.InfusionSlots,
            Weapon weapon => weapon.InfusionSlots,
            Backpack back => back.InfusionSlots,
            Trinket trinket => trinket.InfusionSlots,
            _ => []
        };
    }

    private IEnumerable<InfusionSlot> GetSelectedInfusionSlots()
    {
        return GetInfusionSlots().Zip(
            [_infusionSlot1, _infusionSlot2, _infusionSlot3],
            (infusionSlot, infusionSlotOverride) => infusionSlot with
            {
                ItemId = infusionSlotOverride?.UpgradeComponent?.Id ?? infusionSlotOverride?.DefaultUpgradeComponent?.Id
            });
    }

    private void UpdateChatLink()
    {
        int quantity = _numberPicker.Value;
        _chatLink.Text = (_item.GetChatLink() with
        {
            Count = quantity,
            SuffixItemId = _upgradeSlot1?.UpgradeComponent?.Id,
            SecondarySuffixItemId = _upgradeSlot2?.UpgradeComponent?.Id
        }).ToString();
    }

    private UpgradeComponentsList CreateUpgradeComponentsList(UpgradeSlotType slotType)
    {
        return new UpgradeComponentsList(slotType, _upgrades.Values, _icons, _item)
        {
            Parent = this,
            Width = Width - 50,
            HeightSizingMode = SizingMode.AutoSize,
            Visible = false
        };
    }

    private UpgradeSlot CreateUpgradeSlot(UpgradeSlotType slotType, UpgradeComponent? upgradeComponent)
    {
        return new UpgradeSlot(_icons)
        {
            Parent = this,
            Width = Width,
            HeightSizingMode = SizingMode.AutoSize,
            SlotType = slotType,
            DefaultUpgradeComponent = upgradeComponent,
        };
    }

    private UpgradeSlot? CreateInfusionSlot(InfusionSlot slot)
    {
        UpgradeComponent? component = null;
        if (slot.ItemId.HasValue)
        {
            _upgrades.TryGetValue(slot.ItemId.Value, out component);
        }

        if (slot.Flags.Infusion)
        {
            return CreateUpgradeSlot(UpgradeSlotType.Infusion, component);
        }

        if (slot.Flags.Enrichment)
        {
            return CreateUpgradeSlot(UpgradeSlotType.Enrichment, component);

        }

        return null;
    }

    private UpgradeComponentsList? CreateInfusionList(InfusionSlot slot)
    {
        if (slot.Flags.Infusion)
        {
            return CreateUpgradeComponentsList(UpgradeSlotType.Infusion);
        }

        if (slot.Flags.Enrichment)
        {
            return CreateUpgradeComponentsList(UpgradeSlotType.Enrichment);
        }

        return null;
    }

}
