﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public sealed class ItemWidget : FlowPanel
{
    private readonly TextBox _chatLink;
    private readonly ItemHeader _header;

    private readonly Item _item;

    private readonly TrackBar _quantity;

    public ItemWidget(Item item)
    {
        FlowDirection = ControlFlowDirection.TopToBottom;
        OuterControlPadding = new Vector2(5f);
        Width = 300;
        Height = 530;
        _item = item;
        _header = new ItemHeader(item) { Parent = this, Tooltip = new Tooltip(new ItemTooltipView(item)) };

        ShowTint = true;
        ShowBorder = true;

        Label quantityLabel = new() { Parent = this, Text = "Quantity:", AutoSizeWidth = true, AutoSizeHeight = true };

        _quantity = new TrackBar { Parent = this, Value = 1, MinValue = 1, MaxValue = 250 };

        Label chatLinkLabel = new() { Parent = this, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox { Parent = this, Text = item.ChatLink };

        _header.Click += HeaderClicked;
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
        _header.Quantity = (int)e.Value;
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