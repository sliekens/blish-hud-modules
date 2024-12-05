using System.Text;

using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.GameServices.ArcDps.V2.Models;
using Blish_HUD.Graphics.UI;

using ChatLinksModule.UI.Tabs.Items.Controls;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using static System.Net.Mime.MediaTypeNames;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemTooltipView(Item item) : View, ITooltipView
{
    protected override void Build(Container buildPanel)
    {
        FlowPanel layout = new()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            Width = 350,
            HeightSizingMode = SizingMode.AutoSize,
            Parent = buildPanel
        };

        Header(item, layout);
        WeaponStrength(item, layout);
        Defense(item, layout);
        ItemStats(item, layout);
        Description(item, layout);
        SelectableStats(item, layout);
        Binding(item, layout);
        VendorValue(item, layout);

        static void Header(Item item, Container parent)
        {
            FlowPanel header = new()
            {
                Parent = parent,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5f),
                Width = parent.Width,
                Height = 50
            };

            ItemImage icon = new(item) { Parent = header };

            ItemName name = new(item)
            {
                Parent = header,
                Width = parent.Width - 55,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = GameService.Content.DefaultFont18,
                WrapText = true
            };

            name.Text = name.Text.Replace(" ", "  ");
        }

        static void WeaponStrength(Item item, Container parent)
        {
            if (item is Weapon weapon)
            {
                Label weaponStrength = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = $"Weapon Strength: {weapon.MinPower:N0} - {weapon.MaxPower:N0}"
                };
            }
        }

        static void Defense(Item item, Container parent)
        {
            if (item is Armor armor)
            {
                Label defense = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = $"Defense: {armor.Defense:N0}"
                };
            }
            else if (item is Weapon { Defense: > 0 } weapon)
            {
                Label defense = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = $"Defense: {weapon.Defense:N0}"
                };
            }
        }

        static void ItemStats(Item item, Container parent)
        {
            if (item is Weapon { Attributes.Count: > 0 } weapon)
            {
                StringBuilder builder = new();
                foreach (KeyValuePair<Extensible<AttributeName>, int> stat in weapon.Attributes)
                {
                    builder.AppendFormat("+{0:N0} {1}\r\n", stat.Value, stat.Key);
                }

                Label attributes = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = builder.ToString()
                };
            }
        }


        static void Description(Item item, Container parent)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                return;
            }

            Panel container = new()
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

        static void SelectableStats(Item item, Container parent)
        {
            if (item is Weapon { StatChoices.Count: > 0 }
                or Armor { StatChoices.Count: > 0 }
                or Backpack { StatChoices.Count: > 0 }
                or Trinket { StatChoices.Count: > 0 })
            {
                Label selectableStats = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Double-click to select stats."
                };
            }
        }

        static void Binding(Item item, Container parent)
        {
            if (item is Service)
            {
                return;
            }

            if (item.Flags.Soulbound)
            {
                Label binding = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Soulbound on Acquire"
                };
            }
            else if (item.Flags.SoulbindOnUse)
            {
                Label binding = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Soulbound on Use"
                };
            }
            else if (item.Flags.AccountBound)
            {
                Label binding = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Account Bound on Acquire"
                };
            }
            else if (item.Flags.AccountBindOnUse)
            {
                Label binding = new()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Account Bound on Use"
                };
            }
        }

        static void VendorValue(Item item, Container parent)
        {
            if (item.Flags.NoSell || item.VendorValue == Coin.Zero)
            {
                return;
            }

            FormattedLabelBuilder? builder = new FormattedLabelBuilder()
                .SetWidth(parent.Width)
                .AutoSizeHeight();

            int totalCopper = item.VendorValue.Amount;
            int goldAmount = totalCopper / 10_000;
            totalCopper %= 10_000;
            int silverAmount = totalCopper / 100;
            int copperAmount = totalCopper % 100;

            if (item.VendorValue.Amount >= 10_000)
            {
                FormattedLabelPartBuilder? gold = builder.CreatePart(goldAmount.ToString("N0"));
                gold.SetTextColor(new Color(0xDD, 0xBB, 0x44));
                gold.SetFontSize(ContentService.FontSize.Size14);
                gold.SetSuffixImage(AsyncTexture2D.FromAssetId(156904));
                builder.CreatePart(gold);
            }

            if (item.VendorValue.Amount >= 100)
            {
                FormattedLabelPartBuilder? silver = builder.CreatePart(silverAmount.ToString("N0"));
                silver.SetTextColor(new Color(0xC0, 0xC0, 0xC0));
                silver.SetFontSize(ContentService.FontSize.Size14);
                silver.SetSuffixImage(AsyncTexture2D.FromAssetId(156907));
                builder.CreatePart(silver);
            }

            FormattedLabelPartBuilder? copper = builder.CreatePart(copperAmount.ToString("N0"));
            copper.SetTextColor(new Color(0xCD, 0x7F, 0x32));
            copper.SetFontSize(ContentService.FontSize.Size14);
            copper.SetSuffixImage(AsyncTexture2D.FromAssetId(156902));
            builder.CreatePart(copper);

            FormattedLabel? label = builder.Build();
            label.Parent = parent;
            label.Width = parent.Width;
        }
    }
}