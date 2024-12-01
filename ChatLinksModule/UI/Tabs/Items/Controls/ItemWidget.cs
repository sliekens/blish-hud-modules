using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public sealed class ItemWidget : FlowPanel
{
    private readonly TextBox _chatLink;

    private readonly Item _item;

    private readonly TrackBar _quantity;

    private readonly StandardButton _send;

    public ItemWidget(Item item)
    {
        FlowDirection = ControlFlowDirection.TopToBottom;
        OuterControlPadding = new Vector2(5f);
        Width = 300;
        Height = 530;
        _item = item;
        ItemHeader header = new(item)
        {
            Parent = this,
            Tooltip = new Tooltip(new ItemTooltipView(item))
        };

        ShowTint = true;
        ShowBorder = true;

        Label quantityLabel = new() { Parent = this, Text = "Quantity:", AutoSizeWidth = true, AutoSizeHeight = true };

        _quantity = new TrackBar { Parent = this, Value = 1, MinValue = 1, MaxValue = 255 };

        Label chatLinkLabel = new() { Parent = this, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox
        {
            Parent = this, Text = item.ChatLink
        };

        _send = new StandardButton { Parent = this, Text = "Send to chat", Icon = AsyncTexture2D.FromAssetId(155157) };

        _quantity.ValueChanged += OnQuantityChanged;
        _chatLink.Click += OnChatLinkClick;
        _send.Click += OnSendClick;
    }

    private void OnSendClick(object sender, MouseEventArgs e)
    {
        GameService.GameIntegration.Chat.Send(_chatLink.Text);
    }

    private void OnQuantityChanged(object sender, ValueEventArgs<float> e)
    {
        UpdateChatLink();
    }

    private void OnChatLinkClick(object sender, MouseEventArgs e)
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
        _send.Click -= OnSendClick;
        _quantity.ValueChanged -= OnQuantityChanged;
        _chatLink.Click -= OnChatLinkClick;
        base.DisposeControl();
    }
}