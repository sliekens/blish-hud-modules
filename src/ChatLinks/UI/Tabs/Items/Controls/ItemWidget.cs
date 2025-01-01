using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using SL.Common;
using SL.Common.Controls;
using SL.Common.Controls.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemWidget : FlowPanel
{
    private readonly TextBox _chatLink;

    private readonly Item _item;

    private readonly IReadOnlyDictionary<int, UpgradeComponent> _upgrades;

    private readonly ItemImage _itemIcon;

    private readonly ItemName _itemName;

    private readonly NumberPicker _numberPicker;

    private readonly UpgradeSlots? _upgradeComponents;

    private readonly Label? _infusionWarning;

    public ItemWidget(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        ShowTint = true;
        ShowBorder = true;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(0f, 15f);
        OuterControlPadding = new Vector2(10f);
        AutoSizePadding = new Point(10);
        Width = 350;
        HeightSizingMode = SizingMode.Fill;
        ContentRegion = new Rectangle(5, 5, 290, 520);
        CanScroll = true;

        _item = item;
        _upgrades = upgrades;

        var header = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            ControlPadding = new Vector2(5f),
            Width = Width - 5,
            Height = 50,
            Parent = this
        };

        _itemIcon = new ItemImage(item)
        {
            Parent = header
        };

        _itemIcon.MouseEntered += (_, _) =>
        {
            _itemIcon.Tooltip ??= new Tooltip(new ItemTooltipView(item, upgrades));
        };

        header.Menu = new ItemContextMenu(item);

        _itemName = new ItemName(item, upgrades)
        {
            Parent = header,
            Width = header.Width - 50,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Middle,
            Font = GameService.Content.DefaultFont18,
            WrapText = true
        };

        _itemName.Text = _itemName.Text.Replace(" ", "  ");

        var quantityGroup = new FlowPanel
        {
            Parent = this,
            FlowDirection = ControlFlowDirection.LeftToRight,
            Width = Width,
            HeightSizingMode = SizingMode.AutoSize
        };

        _ = new Label()
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

        StandardButton minQuantity = new()
        {
            Parent = quantityGroup,
            Text = "Min",
            Width = 40,
            Height = Height,
        };

        StandardButton maxQuantity = new()
        {
            Parent = quantityGroup,
            Text = "Max",
            Width = 40,
            Height = Height,
        };

        if (item is IUpgradable)
        {
            _upgradeComponents = new UpgradeSlots(item, upgrades)
            {
                Parent = this,
                Width = Width - 40,
                HeightSizingMode = SizingMode.AutoSize
            };

            _upgradeComponents.ViewModel.UpgradesChanged += () =>
            {
                UpdateHeader();
                UpdateChatLink();
                ToggleWarnings();
            };
        }

        _ = new Label() { Parent = this, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox
        {
            Parent = this,
            Text = item.ChatLink,
            Width = 200
        };

        _infusionWarning = new Label
        {
            Parent = this,
            Width = Width - 20,
            AutoSizeHeight = true,
            WrapText = true,
            TextColor = Color.Yellow,
            Text = """
                   Due to technical restrictions, the game only
                   shows the item's default infusion(s) instead of
                   the selected infusion(s).
                   """,
            Visible = false
        };

        _itemIcon.Click += HeaderClicked;
        _numberPicker.TextChanged += NumberPickerChanged;
        _chatLink.Click += ChatLinkClicked;
        minQuantity.Click += (_, _) => _numberPicker.Value = 1;
        maxQuantity.Click += (_, _) => _numberPicker.Value = 250;
    }

    protected override void OnMouseWheelScrolled(MouseEventArgs e)
    {
        if (_upgradeComponents?.Any(slot => slot.MouseOver) ?? false)
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

    private void UpdateHeader()
    {
        _itemName.SuffixItem = _upgradeComponents?.ViewModel.EffectiveSuffixItem();
        _itemIcon.Tooltip = new Tooltip(new ItemTooltipView(_item switch
        {
            Armor armor => armor with
            {
                Name = armor.NameWithoutSuffix(_upgrades),
                SuffixItemId = _upgradeComponents?.ViewModel.UpgradeSlot1?.EffectiveUpgrade?.Id,
                InfusionSlots = _upgradeComponents?.ViewModel.Infusions() ?? []
            },
            Weapon weapon => weapon with
            {
                Name = weapon.NameWithoutSuffix(_upgrades),
                SuffixItemId = _upgradeComponents?.ViewModel.UpgradeSlot1?.EffectiveUpgrade?.Id,
                SecondarySuffixItemId = _upgradeComponents?.ViewModel.UpgradeSlot2?.EffectiveUpgrade?.Id,
                InfusionSlots = _upgradeComponents?.ViewModel.Infusions() ?? []
            },
            Backpack back => back with
            {
                Name = back.NameWithoutSuffix(_upgrades),
                SuffixItemId = _upgradeComponents?.ViewModel.UpgradeSlot1?.EffectiveUpgrade?.Id,
                InfusionSlots = _upgradeComponents?.ViewModel.Infusions() ?? []
            },
            Trinket trinket => trinket with
            {
                Name = trinket.NameWithoutSuffix(_upgrades),
                SuffixItemId = _upgradeComponents?.ViewModel.UpgradeSlot1?.EffectiveUpgrade?.Id,
                InfusionSlots = _upgradeComponents?.ViewModel.Infusions() ?? []
            },
            _ => _item
        }, _upgrades));
    }

    private void UpdateChatLink()
    {
        int quantity = _numberPicker.Value;
        _chatLink.Text = (_item.GetChatLink() with
        {
            Count = quantity,
            SuffixItemId = _upgradeComponents?.ViewModel.SuffixItemId,
            SecondarySuffixItemId = _upgradeComponents?.ViewModel.SecondarySuffixItemId

        }).ToString();
    }

    private void ToggleWarnings()
    {
        _infusionWarning!.Visible =
            _upgradeComponents?.ViewModel.InfusionSlots?.Any(s => s.SelectedUpgradeComponent != null)
            ?? false;
    }

    private void ChatLinkClicked(object sender, MouseEventArgs e)
    {
        _chatLink.SelectionStart = 0;
        _chatLink.SelectionEnd = _chatLink.Text.Length;
    }
}