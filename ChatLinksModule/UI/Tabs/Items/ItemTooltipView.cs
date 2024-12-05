using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using ChatLinksModule.UI.Tabs.Items.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemTooltipView(Item item) : View, ITooltipView
{
    protected override void Build(Container buildPanel)
    {
        var layout = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            Width = 350,
            HeightSizingMode = SizingMode.AutoSize,
            Parent = buildPanel
        };

        CreateHeader(item, layout);
        CreateDescription(item, layout);

        //if (item.Flags.AccountBound)
        //{
        //    var boundPart = builder.CreatePart("Account Bound on Acquire");
        //    boundPart.SetFontSize(ContentService.FontSize.Size16);
        //    builder.CreatePart(boundPart);
        //}


        static void CreateHeader(Item item, Container parent)
        {
            var header = new FlowPanel
            {
                Parent = parent,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5f),
                Width = parent.Width,
                Height = 50
            };

            ItemImage icon = new(item)
            {
                Parent = header
            };

            ItemName name = new(item)
            {
                Parent = header,
                Width = parent.Width - 55,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = GameService.Content.DefaultFont18,
                WrapText = true,
            };

            name.Text = name.Text.Replace(" ", "  ");
        }

        static void CreateDescription(Item item, Container parent)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                return;
            }

            var container = new Panel
            {
                Parent = parent,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize
            };

            FormattedLabel label = new FormattedLabelBuilder()
                .AddMarkup(item.Description)
                .SetWidth(parent.Width - 10)
                .AutoSizeHeight()
                .Wrap()
                .Build();
            label.Parent = container;
        }
    }

}