using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.GameServices.ArcDps.V2.Models;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public sealed class ItemWidget : FlowPanel
{
    private readonly TextBox _chatLink;

    private readonly Item _item;

    private readonly ItemImage _itemIcon;

    private readonly ItemName _itemName;

    private readonly TrackBar _quantity;

    public ItemWidget(Item item)
    {
        ShowTint = true;
        ShowBorder = true;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(10);
        Width = 350;
        HeightSizingMode = SizingMode.AutoSize;
        ContentRegion = new Rectangle(5, 5, 290, 520);

        _item = item;

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
            Parent = header,
            Tooltip = new Tooltip(new ItemTooltipView(item))
        };

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

        Label quantityLabel = new() { Parent = this, Text = "Quantity:", AutoSizeWidth = true, AutoSizeHeight = true };

        _quantity = new TrackBar
        {
            Parent = this, Value = 1, MinValue = 1, MaxValue = 250,
            Width = Width - 10
        };

        Label chatLinkLabel = new() { Parent = this, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox
        {
            Parent = this, Text = item.ChatLink,
            Width = Width
        };

        _itemIcon.Click += HeaderClicked;
        _quantity.ValueChanged += QuantityChanged;
        _chatLink.Click += ChatLinkClicked;
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

    private void QuantityChanged(object sender, ValueEventArgs<float> e)
    {
        _itemName.Quantity = (int)e.Value;
        UpdateChatLink();
    }

    private void ChatLinkClicked(object sender, MouseEventArgs e)
    {
        _chatLink.SelectionStart = 0;
        _chatLink.SelectionEnd = _chatLink.Text.Length;
    }

    private void UpdateChatLink()
    {
        int quantity = (int)_quantity.Value;
        _chatLink.Text = (_item.GetChatLink() with { Count = quantity }).ToString();
    }

    protected override void DisposeControl()
    {
        _quantity.ValueChanged -= QuantityChanged;
        _chatLink.Click -= ChatLinkClicked;
        base.DisposeControl();
    }
}