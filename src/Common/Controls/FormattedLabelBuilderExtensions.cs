﻿using System.Globalization;

using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Markup;

using Microsoft.Xna.Framework;

namespace SL.Common.Controls;

public static class FormattedLabelBuilderExtensions
{
    private static readonly MarkupLexer Lexer = new();
    private static readonly MarkupParser Parser = new();

    public static FormattedLabelBuilder AddMarkup(this FormattedLabelBuilder builder, string markup, Color? primaryColor = null)
    {
        IEnumerable<MarkupToken> tokens = Lexer.Tokenize(markup);
        RootNode syntax = Parser.Parse(tokens);
        foreach (var part in syntax.Children.SelectMany(node => builder.CreateParts(node, primaryColor ?? Color.White)))
        {
            part.SetFontSize(ContentService.FontSize.Size16);
            builder.CreatePart(part);
        }

        return builder;
    }

    private static IEnumerable<FormattedLabelPartBuilder> CreateParts(this FormattedLabelBuilder builder, MarkupNode node, Color currentColor)
    {
        switch (node.Type)
        {
            case MarkupNodeType.Text:
                var text = (TextNode)node;
                var textPart = builder.CreatePart(text.Text);
                textPart.SetTextColor(currentColor);
                yield return textPart;
                break;
            case MarkupNodeType.LineBreak:
                yield return builder.CreatePart("\r\n");
                break;
            case MarkupNodeType.ColoredText:
                var coloredText = (ColoredTextNode)node;
                var color = ParseColor(coloredText.Color);
                foreach (var part in coloredText.Children.SelectMany(child => builder.CreateParts(child, color)))
                {
                    yield return part;
                }
                break;
        }
    }

    private static Color ParseColor(string color)
    {
        if (MarkupColorName.DefaultColorMap.TryGetValue(color, out var colorCode))
        {
            color = colorCode;
        }

        if (color.StartsWith("#", StringComparison.Ordinal))
        {
            var hex = color[1..];
            try
            {
                byte r = byte.Parse(hex[..2], NumberStyles.HexNumber);
                byte g = byte.Parse(hex[2..4], NumberStyles.HexNumber);
                byte b = byte.Parse(hex[4..6], NumberStyles.HexNumber);
                return new Color(r, g, b);
            }
            catch (FormatException)
            {
            }
        }

        return Color.White;
    }
}
