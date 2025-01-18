using System.ComponentModel;

using Blish_HUD.Controls;
using Blish_HUD.Input;

namespace SL.ChatLinks.UI;

public sealed class MainIcon : CornerIcon
{
    public MainIconViewModel ViewModel { get; }

    public MainIcon(MainIconViewModel viewModel)
        : base(viewModel.Texture, viewModel.HoverTexture, viewModel.Name)
    {
        ViewModel = viewModel;
        Parent = Graphics.SpriteScreen;
        Priority = viewModel.Priority;
        viewModel.Initialize();
        viewModel.PropertyChanged += ModelPropertyChanged;
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
