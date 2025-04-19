using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

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

        int chatLinkWidth = 170;
        _detailsButton.Icon = viewModel.Locked
            ? AsyncTexture2D.FromAssetId(240704)
            : viewModel.GetIcon() ?? AsyncTexture2D.FromAssetId(155865);

        if (viewModel.Progression is null)
        {
            // Show all achievements as if they are not even started

            Image info = new()
            {
                Parent = toolbar,
                Size = new(32),
                Texture = AsyncTexture2D.FromAssetId(1508665),
                BasicTooltipText = ViewModel.AchievementProgressUnavailable
            };

            chatLinkWidth -= info.Width;

            if (!viewModel.Locked)
            {
                _detailsButton.MaxFill = viewModel.Achievement.Tiers[0].Count;
                _detailsButton.ShowFillFraction = true;
            }
        }
        else if (viewModel.Progress is null)
        {
            // Progress is always unavailable for daily, weekly, monthly and per-character achievements
            if (ViewModel.IsDaily)
            {
                Image info = new()
                {
                    Parent = toolbar,
                    Size = new(32),
                    Texture = AsyncTexture2D.FromAssetId(1508665),
                    BasicTooltipText = ViewModel.DailyAchievementProgressUnavailable
                };

                chatLinkWidth -= info.Width;
            }
            else if (viewModel.IsWeekly)
            {
                Image info = new()
                {
                    Parent = toolbar,
                    Size = new(32),
                    Texture = AsyncTexture2D.FromAssetId(1508665),
                    BasicTooltipText = ViewModel.WeeklyAchievementProgressUnavailable
                };

                chatLinkWidth -= info.Width;
            }
            else if (viewModel.IsMonthly)
            {
                Image info = new()
                {
                    Parent = toolbar,
                    Size = new(32),
                    Texture = AsyncTexture2D.FromAssetId(1508665),
                    BasicTooltipText = ViewModel.MonthlyAchievementProgressUnavailable
                };

                chatLinkWidth -= info.Width;
            }
            else if (viewModel.IsPerCharacter)
            {
                Image info = new()
                {
                    Parent = toolbar,
                    Size = new(32),
                    Texture = AsyncTexture2D.FromAssetId(1508665),
                    BasicTooltipText = ViewModel.PerCharacterAchievementProgressUnavailable
                };

                chatLinkWidth -= info.Width;
            }
            else if (!viewModel.Locked)
            {
                // Still no progress probably just means the achievement is not started
                _detailsButton.MaxFill = viewModel.Achievement.Tiers[0].Count;
                _detailsButton.ShowFillFraction = true;
            }
        }
        else if (!viewModel.Progress.Done || viewModel.Achievement.Flags.Repeatable)
        {
            _detailsButton.MaxFill = viewModel.Progress.Max;
            _detailsButton.CurrentFill = viewModel.Progress.Current;
            _detailsButton.ShowFillFraction = true;
        }
        else
        {
            _detailsButton.ShowVignette = false;
            _detailsButton.IconDetails = viewModel.CompletedLabel;
        }

        if (ViewModel.Achievement.Flags.Hidden)
        {
            StackedImage eye = new()
            {
                Parent = toolbar,
                Size = new(32),
                BasicTooltipText = viewModel.HiddenLabel
            };

            eye.Textures.Add((AsyncTexture2D.FromAssetId(528726), Color.White));

            if (viewModel.Progress is null)
            {
                eye.Textures.Add((AsyncTexture2D.FromAssetId(154983), Color.OrangeRed));
            }

            chatLinkWidth -= eye.Width;
        }

        _chatLink = new()
        {
            Parent = toolbar,
            Height = 35,
            Width = chatLinkWidth,
            Text = viewModel.ChatLink,
            HideBackground = true
        };

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
        Soundboard.Click();
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
