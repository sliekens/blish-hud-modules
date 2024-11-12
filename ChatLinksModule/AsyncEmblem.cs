using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace MyBlishHUDModule;

internal sealed class AsyncEmblem : IDisposable
{
	private readonly AsyncTexture2D _emblem;

	private readonly WindowBase2 _window;

	private AsyncEmblem(WindowBase2 window, AsyncTexture2D emblem)
	{
		_window = window;
		_emblem = emblem;
	}

	public void Dispose()
	{
		_emblem.TextureSwapped -= OnTextureSwapped;
	}

	public static AsyncEmblem Attach(WindowBase2 window, AsyncTexture2D emblem)
	{
		var asyncEmblem = new AsyncEmblem(window, emblem);
		if (emblem.HasSwapped)
			window.Emblem = emblem.Texture;
		else
			emblem.TextureSwapped += asyncEmblem.OnTextureSwapped;

		return asyncEmblem;
	}

	private void OnTextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
	{
		_window.Emblem = e.NewValue;
	}
}