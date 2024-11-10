using System;
using System.ComponentModel.Composition;
using Blish_HUD;
using Blish_HUD.Modules;

namespace MyBlishHUDModule;

[Export(typeof(Module))]
public class MyModule : Module
{
	private static readonly Logger Logger = Logger.GetLogger<MyModule>();

	[ImportingConstructor]
	public MyModule([Import("ModuleParameters")] ModuleParameters parameters)
		: base(parameters)
	{
	}
}