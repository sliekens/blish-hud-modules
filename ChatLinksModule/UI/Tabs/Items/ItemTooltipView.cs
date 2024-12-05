using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using ChatLinksModule.UI.Tabs.Items.Controls;

using GuildWars2;
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
        CreateBinding(item, layout);
        CreateVendorValue(item, layout);


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

        static void CreateBinding(Item item, Container parent)
        {
            if (item.Flags.NoSell || item is Service)
            {
                return;
            }

            if (item.Flags.Soulbound)
            {
                var binding = new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Soulbound on Acquire"
                };
            }
            else if (item.Flags.SoulbindOnUse)
            {
                var binding = new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Soulbound on Use"
                };
            }
            else if (item.Flags.AccountBound)
            {
                var binding = new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Account Bound on Acquire"
                };
            }
            else if (item.Flags.AccountBindOnUse)
            {
                var binding = new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Account Bound on Use"
                };
            }
        }

        static void CreateVendorValue(Item item, Container parent)
        {
            if (item.Flags.NoSell || item.VendorValue == Coin.Zero)
            {
                return;
            }

            var builder = new FormattedLabelBuilder()
                .SetWidth(parent.Width)
                .AutoSizeHeight();

            int totalCopper = item.VendorValue.Amount;
            int goldAmount = totalCopper / 10_000;
            totalCopper %= 10_000;
            int silverAmount = totalCopper / 100;
            int copperAmount = totalCopper % 100;

            if (item.VendorValue.Amount >= 10_000)
            {
                var gold = builder.CreatePart(goldAmount.ToString());
                gold.SetTextColor(new Color(0xDD, 0xBB, 0x44));
                gold.SetFontSize(ContentService.FontSize.Size14);
                gold.SetSuffixImage(AsyncTexture2D.FromAssetId(156904));
                builder.CreatePart(gold);
            }

            if (item.VendorValue.Amount >= 100)
            {
                var silver = builder.CreatePart(silverAmount.ToString());
                silver.SetTextColor(new Color(0xC0, 0xC0, 0xC0));
                silver.SetFontSize(ContentService.FontSize.Size14);
                silver.SetSuffixImage(AsyncTexture2D.FromAssetId(156907));
                builder.CreatePart(silver);
            }

            var copper = builder.CreatePart(copperAmount.ToString());
            copper.SetTextColor(new Color(0xCD, 0x7F, 0x32));
            copper.SetFontSize(ContentService.FontSize.Size14);
            copper.SetSuffixImage(AsyncTexture2D.FromAssetId(156902));
            builder.CreatePart(copper);

            var label = builder.Build();
            label.Parent = parent;
            label.Width = parent.Width;
        }
    }
}