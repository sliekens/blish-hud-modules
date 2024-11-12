using System;
using System.ComponentModel.Composition;
using Blish_HUD.Modules;
using ChatLinksModule.UI;

namespace ChatLinksModule;

[Export(typeof(Module))]
[method: ImportingConstructor]
public class ChatLinksModule([Import("ModuleParameters")] ModuleParameters parameters) : Module(parameters)
{
	private MainIcon _cornerIcon;

	private MainWindow _mainWindow;

	protected override void Initialize()
	{
		_mainWindow = MainWindow.Create(ModuleParameters);
		_cornerIcon = MainIcon.Create(ModuleParameters);
		_cornerIcon.Click += CornerIcon_Click;
	}

	private void CornerIcon_Click(object sender, EventArgs e)
	{
		_mainWindow.ToggleWindow();
	}

	protected override void Unload()
	{
		_cornerIcon.Click -= CornerIcon_Click;
		_cornerIcon.Dispose();
		_mainWindow.Dispose();
	}
}