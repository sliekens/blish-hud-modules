using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Hero.Achievements;

namespace SL.ChatLinks.UI.Tabs.Achievements;
public sealed class AchievementTile : Container
{
    private readonly TextBox _chatLink = new();

    public AchievementTile(Achievement achievement)
    {
        ThrowHelper.ThrowIfNull(achievement);
        Width = 320;
        Height = 90;
        DetailsButton button = new()
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill,
            Text = achievement.Name
        };

        if (!string.IsNullOrEmpty(achievement.Description))
        {
            button.BasicTooltipText = achievement.Description;
        }

        if (!string.IsNullOrEmpty(achievement.IconHref))
        {
            button.Icon = GameService.Content.GetRenderServiceTexture(achievement.IconHref);
        }

        _chatLink = new()
        {
            Parent = button,
            Width = 230,
            Height = 35,
            Left = 0,
            Text = achievement.GetChatLink().ToString()
        };

        _chatLink.InputFocusChanged += ChatLinkFocusChanged;
    }

    private void ChatLinkFocusChanged(object sender, ValueEventArgs<bool> args)
    {
        if (args.Value)
        {
            _chatLink.SelectionStart = 0;
            _chatLink.SelectionEnd = _chatLink.Length;
        }
        else
        {
            _chatLink.SelectionStart = _chatLink.SelectionEnd;
        }
    }
}
