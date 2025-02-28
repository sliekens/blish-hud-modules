using Blish_HUD;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Achievements.Tooltips;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementTile : Container
{
    private readonly DetailsButton _detailsButton;

    private readonly TextBox _chatLink = new();

    public AchievementTileViewModel ViewModel { get; }

    public AchievementTile(AchievementTileViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        Width = 320;
        Height = 90;
        _detailsButton = new DetailsButton()
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill,
            Text = viewModel.Name
        };

        if (!string.IsNullOrEmpty(viewModel.IconHref))
        {
            _detailsButton.Icon = GameService.Content.GetRenderServiceTexture(viewModel.IconHref);
        }

        _chatLink = new()
        {
            Parent = _detailsButton,
            Width = 230,
            Height = 35,
            Left = 0,
            Text = viewModel.ChatLink
        };

        _chatLink.InputFocusChanged += ChatLinkFocusChanged;
        Menu = new ContextMenuStrip(() =>
        [
            ViewModel.CopyNameCommand.ToMenuItem(() => ViewModel.CopyNameLabel),
            ViewModel.CopyChatLinkCommand.ToMenuItem(() => viewModel.CopyChatLinkLabel),
            ViewModel.OpenWikiCommand.ToMenuItem(() => viewModel.OpenWikiLabel),
            ViewModel.OpenApiCommand.ToMenuItem(() => viewModel.OpenApiLabel)
        ]);
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

    public override void UpdateContainer(GameTime gameTime)
    {
        if (MouseOver)
        {
            _detailsButton.Tooltip ??= new Tooltip(new AchievementTooltipView(ViewModel.CreateAchievementTooltipViewModel()));
        }
        else
        {
            _detailsButton.Tooltip?.Dispose();
            _detailsButton.Tooltip = null;
        }

        base.UpdateContainer(gameTime);
    }

    protected override void DisposeControl()
    {
        ViewModel.Dispose();
        base.DisposeControl();
    }
}
