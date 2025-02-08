using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ChatLinkEditorView(ChatLinkEditorViewModel viewModel) : View
{
    private readonly ChatLinkEditor _chatLinkEditor = new(viewModel);

    protected override void Build(Container buildPanel)
    {
        _chatLinkEditor.Parent = buildPanel;
    }

    protected override void Unload()
    {
        _chatLinkEditor.Dispose();
    }
}