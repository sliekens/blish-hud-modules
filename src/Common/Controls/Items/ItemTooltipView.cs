using System.Text;

using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace SL.Common.Controls.Items;

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
        Hint(item, layout);
        WeaponStrength(item, layout);
        Defense(item, layout);
        ItemStats(item, layout);
        Effect(item, layout);
        Description(item, layout);
        SelectableStats(item, layout);
        TypeName(item, layout);
        RequiredLevel(item, layout);
        Binding(item, layout);
        if (!item.Flags.NoSell)
        {
            VendorValue(item.VendorValue, layout);
        }

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

        static Control? Hint(Item item, Container parent)
        {
            return item switch
            {
                Currency or Service => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Takes effect immediately upon receipt."
                },
                Consumable => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Double-click to consume."
                },
                _ => null
            };
        }

        static Control? WeaponStrength(Item item, Container parent)
        {
            return item switch
            {
                Weapon weapon => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = $"Weapon Strength: {weapon.MinPower:N0} - {weapon.MaxPower:N0}"
                },
                _ => null
            };
        }

        static Control? Defense(Item item, Container parent)
        {
            return item switch
            {
                Armor armor => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = $"Defense: {armor.Defense:N0}"
                },
                Weapon { Defense: > 0 } weapon => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = $"Defense: {weapon.Defense:N0}"
                },
                _ => null
            };
        }

        static Control? ItemStats(Item item, Container parent)
        {
            return item switch
            {
                Weapon { Attributes.Count: > 0 } weapon => Attributes(weapon.Attributes, parent),
                Armor { Attributes.Count: > 0 } armor => Attributes(armor.Attributes, parent),
                Backpack { Attributes.Count: > 0 } backpack => Attributes(backpack.Attributes, parent),
                Trinket { Attributes.Count: > 0 } trinket => Attributes(trinket.Attributes, parent),
                UpgradeComponent { Attributes.Count: > 0 } upgradeComponent => Attributes(upgradeComponent.Attributes,
                    parent, new Color(0x55, 0x99, 0xFF)),
                _ => null
            };

            static Label Attributes(IDictionary<Extensible<AttributeName>, int> attributes, Container parent,
                Color? textColor = null)
            {
                StringBuilder builder = new();
                foreach (KeyValuePair<Extensible<AttributeName>, int> stat in attributes)
                {
                    builder.AppendFormat("+{0:N0} {1}\r\n", stat.Value, stat.Key);
                }

                return new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = builder.ToString(),
                    TextColor = textColor.GetValueOrDefault(Color.White)
                };
            }
        }

        static Control? Effect(Item item, Container parent)
        {
            return item switch
            {
                GenericConsumable { Effect: { } effect } => Effect(effect, parent),
                Food { Effect: { } effect } => Effect(effect, parent),
                Utility { Effect: { } effect } => Effect(effect, parent),
                Service { Effect: { } effect } => Effect(effect, parent),
                _ => null
            };

            static Control Effect(Effect effect, Container parent)
            {
                var panel = new FlowPanel
                {
                    Parent = parent,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                    Width = parent.Width,
                    HeightSizingMode = SizingMode.AutoSize,
                    ControlPadding = new Vector2(5f)
                };

                if (!string.IsNullOrEmpty(effect.IconHref))
                {
                    var icon = new Image
                    {
                        Parent = panel,
                        Texture = GameService.Content.GetRenderServiceTexture(effect.IconHref),
                        Size = new Point(32)
                    };
                }

                var builder = new StringBuilder();
                builder.Append(effect.Name);

                if (effect.Duration > TimeSpan.Zero)
                {
                    builder.AppendFormat(" ({0})", effect.Duration switch
                    {
                        { Hours: >= 1 } => $"{effect.Duration.TotalHours} h",
                        _ => $"{effect.Duration.TotalMinutes} m"
                    });
                }

                builder.Append(": ");
                builder.Append(effect.Description);

                var label = new Label
                {
                    Parent = panel,
                    Width = panel.Width - 30,
                    AutoSizeHeight = true,
                    WrapText = true,
                    Text = builder.ToString(),
                    TextColor = new Color(0xAA, 0xAA, 0xAA)
                };

                return panel;
            }
        }

        static Control? Description(Item item, Container parent)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                return null;
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
            return label;
        }

        static Control? SelectableStats(Item item, Container parent)
        {
            if (item is Weapon { StatChoices.Count: > 0 }
                or Armor { StatChoices.Count: > 0 }
                or Backpack { StatChoices.Count: > 0 }
                or Trinket { StatChoices.Count: > 0 })
            {
                return new Label()
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Double-click to select stats."
                };
            }

            return null;
        }

        static Control? RequiredLevel(Item item, Container parent)
        {
            if (item.Level == 0)
            {
                return null;
            }

            return new Label
            {
                Parent = parent,
                Width = parent.Width,
                AutoSizeHeight = true,
                Text = $"Required Level: {item.Level}"
            };
        }

        static Control? TypeName(Item item, Container parent)
        {
            return item switch
            {
                Miniature => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Mini"
                },
                Service => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Service"
                },
                Consumable => new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Consumable"
                },
                _ => null
            };
        }

        static Control? Binding(Item item, Container parent)
        {
            if (item is Currency or Service)
            {
                return null;
            }

            if (item.Flags.Soulbound)
            {
                return new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Soulbound on Acquire"
                };
            }

            if (item.Flags.SoulbindOnUse)
            {
                return new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Soulbound on Use"
                };
            }

            if (item.Flags.AccountBound)
            {
                return new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Account Bound on Acquire"
                };
            }

            if (item.Flags.AccountBindOnUse)
            {
                return new Label
                {
                    Parent = parent,
                    Width = parent.Width,
                    AutoSizeHeight = true,
                    Text = "Account Bound on Use"
                };
            }

            return null;
        }

        static Control? VendorValue(Coin vendorValue, Container parent)
        {
            if (vendorValue == Coin.Zero)
            {
                return null;
            }

            FormattedLabelBuilder? builder = new FormattedLabelBuilder()
                .SetWidth(parent.Width)
                .AutoSizeHeight();

            if (vendorValue.Amount >= 10_000)
            {
                FormattedLabelPartBuilder? gold = builder.CreatePart(vendorValue.Gold.ToString("N0"));
                gold.SetTextColor(new Color(0xDD, 0xBB, 0x44));
                gold.SetFontSize(ContentService.FontSize.Size14);
                gold.SetSuffixImage(AsyncTexture2D.FromAssetId(156904));
                builder.CreatePart(gold);
            }

            if (vendorValue.Amount >= 100)
            {
                FormattedLabelPartBuilder? silver = builder.CreatePart(vendorValue.Silver.ToString("N0"));
                silver.SetTextColor(new Color(0xC0, 0xC0, 0xC0));
                silver.SetFontSize(ContentService.FontSize.Size14);
                silver.SetSuffixImage(AsyncTexture2D.FromAssetId(156907));
                builder.CreatePart(silver);
            }

            FormattedLabelPartBuilder? copper = builder.CreatePart(vendorValue.Copper.ToString("N0"));
            copper.SetTextColor(new Color(0xCD, 0x7F, 0x32));
            copper.SetFontSize(ContentService.FontSize.Size14);
            copper.SetSuffixImage(AsyncTexture2D.FromAssetId(156902));
            builder.CreatePart(copper);

            FormattedLabel? label = builder.Build();
            label.Parent = parent;
            label.Width = parent.Width;

            return label;
        }
    }
}
