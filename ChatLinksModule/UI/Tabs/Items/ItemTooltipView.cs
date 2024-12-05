using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
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
            WidthSizingMode = SizingMode.AutoSize,
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
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5f),
                Width = 290,
                Height = 50,
                Parent = parent
            };

            ItemImage icon = new(item)
            {
                Parent = header
            };

            ItemName name = new(item)
            {
                Parent = header,
                Width = 235,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = GameService.Content.DefaultFont18,
                WrapText = true,
            };
        }

        static void CreateDescription(Item item, Container parent)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                return;
            }

            FormattedLabel label = new FormattedLabelBuilder()
                .Wrap()
                .SetWidth(350)
                .AutoSizeHeight()
                .AddMarkup(item.Description)
                .Build();
            label.Parent = parent;
        }
    }

}