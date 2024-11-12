using System;
using System.ComponentModel.Composition;
using Blish_HUD.Modules;

namespace ChatLinksModule;

[Export(typeof(Module))]
[method: ImportingConstructor]
public class MyModule([Import("ModuleParameters")] ModuleParameters parameters) : Module(parameters)
{
	private MyCornerIcon _cornerIcon;

	private MyMainWindow _mainWindow;

	protected override void Initialize()
	{
		_mainWindow = MyMainWindow.Create(ModuleParameters);
		_cornerIcon = MyCornerIcon.Create(ModuleParameters);
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