using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Hero.Achievements;

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

        FlowPanel toolbar = new()
        {
            Parent = _detailsButton,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill,
            FlowDirection = ControlFlowDirection.SingleLeftToRight
        };

        if (viewModel.Locked)
        {
            _detailsButton.Icon = AsyncTexture2D.FromAssetId(240704);
        }
        else if (!string.IsNullOrEmpty(viewModel.IconHref))
        {
            _detailsButton.Icon = GameService.Content.GetRenderServiceTexture(viewModel.IconHref);
            AccountAchievement? progress = viewModel.Progress;
            if (progress is not null)
            {
                if (!viewModel.Achievement.Flags.Repeatable && progress.Done)
                {
                    _detailsButton.ShowVignette = false;
                    _detailsButton.IconDetails = viewModel.CompletedLabel;
                }
                else
                {
                    _detailsButton.MaxFill = progress.Max;
                    _detailsButton.CurrentFill = progress.Current;
                    _detailsButton.ShowFillFraction = true;
                }
            }
        }

        if (ViewModel.Progress is null)
        {
            Image warning = new()
            {
                Parent = toolbar,
                Size = new(32),
                Texture = AsyncTexture2D.FromAssetId(1508665),
                BasicTooltipText = ViewModel.MissingProgressWarning
            };
        }

        _chatLink = new()
        {
            Parent = toolbar,
            Height = 35,
            Width = 170,
            Text = viewModel.ChatLink,
            HideBackground = true
        };

        if (ViewModel.Progress is null)
        {
            _chatLink.Width -= 32;
        }

        _chatLink.InputFocusChanged += ChatLinkFocusChanged;

        GlowButton copyButton = new()
        {
            Parent = toolbar,
            Icon = AsyncTexture2D.FromAssetId(2208345),
            ActiveIcon = AsyncTexture2D.FromAssetId(2208347)
        };

        copyButton.Click += OnCopyClicked;

        Menu = new ContextMenuStrip(() =>
        [
            ViewModel.CopyNameCommand.ToMenuItem(() => ViewModel.CopyNameLabel),
            ViewModel.CopyChatLinkCommand.ToMenuItem(() => viewModel.CopyChatLinkLabel),
            ViewModel.OpenWikiCommand.ToMenuItem(() => viewModel.OpenWikiLabel),
            ViewModel.OpenApiCommand.ToMenuItem(() => viewModel.OpenApiLabel)
        ]);
    }

    private void OnCopyClicked(object sender, MouseEventArgs e)
    {
        _ = Soundboard.Click.Play();
        ViewModel.CopyChatLinkCommand.Execute();
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
