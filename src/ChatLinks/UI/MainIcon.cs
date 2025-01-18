using System.ComponentModel;

using Blish_HUD.Controls;
using Blish_HUD.Input;

using SL.Common.Controls;

namespace SL.ChatLinks.UI;

public sealed class MainIcon : CornerIcon
{
    public MainIconViewModel ViewModel { get; }

    public MainIcon(MainIconViewModel viewModel)
        : base(viewModel.Texture, viewModel.HoverTexture, viewModel.Name)
    {
        Parent = Graphics.SpriteScreen;
        Priority = viewModel.Priority;
        ViewModel = viewModel;
        viewModel.Initialize();
        viewModel.PropertyChanged += ModelPropertyChanged;
        PropertyChanged += ViewPropertyChanged;

        Menu = new ContextMenuStrip();
        Menu.AddMenuItem(ViewModel.SyncCommand.ToMenuItem(() => "Sync database"));
    }

    private void ViewPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(LoadingMessage):
                ViewModel.LoadingMessage = LoadingMessage;
                break;
            case nameof(BasicTooltipText):
                ViewModel.TooltipText = BasicTooltipText;
                break;
        }
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(ViewModel.LoadingMessage):
                LoadingMessage = ViewModel.LoadingMessage;
                break;
            case nameof(ViewModel.TooltipText):
                BasicTooltipText = ViewModel.TooltipText;
                break;
        }
    }

    protected override void OnClick(MouseEventArgs e)
    {
        ViewModel.ClickCommand.Execute();
        base.OnClick(e);
    }
}
