﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using SL.Common.Controls;
using SL.Common.Controls.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemWidget : FlowPanel
{
	private readonly TextBox _chatLink;

	private readonly Item _item;

	private readonly ItemImage _itemIcon;

	private readonly ItemName _itemName;

	private readonly NumberPicker _numberPicker;

	public ItemWidget(Item item, List<UpgradeComponent> upgrades)
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
			Tooltip = new Tooltip(new ItemTooltipView(item, upgrades))
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

		FormattedLabel link = new FormattedLabelBuilder().SetWidth(Width)
			.AutoSizeHeight()
			.Wrap()
			.CreatePart("API", part => part.SetHyperLink($"https://api.guildwars2.com/v2/items/{item.Id}?v=latest"))
			.Build();

		link.Parent = this;

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
			Width = Width
		};

		_itemIcon.Click += HeaderClicked;
        _numberPicker.TextChanged += NumberPickerChanged;
		_chatLink.Click += ChatLinkClicked;
        minQuantity.Click += MinQuantityOnClick;
        maxQuantity.Click += MaxQuantityOnClick;

        void MinQuantityOnClick(object sender, MouseEventArgs e)
        {
            _numberPicker.Value = 1;
        }

        void MaxQuantityOnClick(object sender, MouseEventArgs e)
        {
            _numberPicker.Value = 250;
        }
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

	private void UpdateChatLink()
	{
		int quantity = _numberPicker.Value;
		_chatLink.Text = (_item.GetChatLink() with { Count = quantity }).ToString();
	}

	protected override void DisposeControl()
	{
		_chatLink.Click -= ChatLinkClicked;
		base.DisposeControl();
	}
}
