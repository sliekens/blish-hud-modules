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
        PropertyChanged += ViewPropertyChanged;

        Menu = new ContextMenuStrip();

        var bananaModeItem = Menu.AddMenuItem(ViewModel.BananaModeLabel);
        bananaModeItem.CanCheck = true;
        bananaModeItem.Checked = viewModel.BananaMode;
        bananaModeItem.CheckedChanged += (sender, args) =>
        {
            ViewModel.BananaMode = args.Checked;
        };

        var raiseStackSizeItem = Menu.AddMenuItem(ViewModel.RaiseStackSizeLabel);
        raiseStackSizeItem.CanCheck = true;
        raiseStackSizeItem.Checked = viewModel.RaiseStackSize;
        raiseStackSizeItem.CheckedChanged += (sender, args) =>
        {
            ViewModel.RaiseStackSize = args.Checked;
        };

        var syncItem = ViewModel.SyncCommand.ToMenuItem(() => ViewModel.SyncLabel);
        Menu.AddMenuItem(syncItem);


        var koFiItem = ViewModel.KoFiCommand.ToMenuItem(() => ViewModel.KoFiLabel);
        Menu.AddMenuItem(koFiItem);

        viewModel.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.Name):
                    IconName = viewModel.Name;
                    break;
                case nameof(ViewModel.BananaModeLabel):
                    bananaModeItem.Text = viewModel.BananaModeLabel;
                    break;
                case nameof(ViewModel.BananaMode):
                    bananaModeItem.Checked = viewModel.BananaMode;
                    break;
                case nameof(ViewModel.RaiseStackSizeLabel):
                    raiseStackSizeItem.Text = viewModel.RaiseStackSizeLabel;
                    break;
                case nameof(ViewModel.RaiseStackSize):
                    raiseStackSizeItem.Checked = viewModel.RaiseStackSize;
                    break;
                case nameof(ViewModel.SyncLabel):
                    syncItem.Text = viewModel.SyncLabel;
                    break;
                case nameof(ViewModel.KoFiLabel):
                    koFiItem.Text = viewModel.KoFiLabel;
                    break;
                case nameof(ViewModel.LoadingMessage):
                    LoadingMessage = ViewModel.LoadingMessage;
                    break;
                case nameof(ViewModel.TooltipText):
                    BasicTooltipText = ViewModel.TooltipText;
                    break;
            }
        };
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

    protected override void OnClick(MouseEventArgs e)
    {
        ViewModel.ClickCommand.Execute();
        base.OnClick(e);
    }
}
