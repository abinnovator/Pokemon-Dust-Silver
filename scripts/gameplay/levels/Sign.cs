using Game.Core;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System;
using Logger = Game.Core.Logger;
using System.Threading.Tasks;


namespace Game.Gameplay;
[Tool]
public partial class Sign : StaticBody2D
{
	[Export]
	public Array<string> Messages;
	private SignType _signStyle = SignType.METAL;

	[Export]
	public SignType SignStyle
	{
		get => _signStyle;
		set 
		{
			if (_signStyle != value)
			{
				if (_signStyle != value)
				{
					_signStyle = value;
					UpdateSprite();
				}
			}
		}
	}

	private Sprite2D _sprite2D;

	private readonly System.Collections.Generic.Dictionary<SignType, AtlasTexture> _textures = new ()
	{
		{ SignType.METAL, GD.Load<AtlasTexture>("res://resources/textures/sign_metal.tres") },
		{ SignType.SNOWY_METAL, GD.Load<AtlasTexture>("res://resources/textures/sign_snowy_metal.tres") },
		{ SignType.WOOD, GD.Load<AtlasTexture>("res://resources/textures/sign_wood.tres") },
		{ SignType.SNOWY_WOOD, GD.Load<AtlasTexture>("res://resources/textures/sign_snowy_wood.tres") },
		{ SignType.LARGE_WOOD, GD.Load<AtlasTexture>("res://resources/textures/sign_large_wood.tres") },
		{ SignType.SNOWY_LARGE_WOOD, GD.Load<AtlasTexture>("res://resources/textures/sign_large_snowy_wood.tres") },
		{ SignType.LARGE_METAL, GD.Load<AtlasTexture>("res://resources/textures/sign_large_metal.tres") },
		{ SignType.SNOWY_LARGE_METAL, GD.Load<AtlasTexture>("res://resources/textures/sign_snowy_large_metal.tres") }
	};
	public override void _Ready()
	{
		_sprite2D ??= GetNode<Sprite2D>("Sprite2D");
		UpdateSprite();
	}
	private void UpdateSprite()
	{
		if (_sprite2D == null){
			_sprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");
			if (_sprite2D == null) {
				Logger.Info("Sprite2D node not found");
				return;
			}
		}
		if (_textures.TryGetValue(SignStyle, out var texture))
		{
			_sprite2D.Texture = texture;
		}
		else {
			Logger.Info("Sign style not found");
			_sprite2D.Texture = _textures[SignType.METAL];
		}
	}
	// Change this to an async Task so the game knows to wait
	private bool _isInteracting = false; 

	public async Task PlayMessage()
	{
		// 1. Prevent overlapping interactions
		if (_isInteracting) return;
		_isInteracting = true;

		// 2. Await the message manager
		// We use [..Messages] to convert your Godot Array to a C# params array
		await MessageManager.PlayText([..Messages]);

		// 3. The "Cooldown": Wait a tiny bit before allowing interaction again
		// This stops the "Space" button from instantly re-triggering the sign.
		await Task.Delay(200); 

		_isInteracting = false;
	}
}
