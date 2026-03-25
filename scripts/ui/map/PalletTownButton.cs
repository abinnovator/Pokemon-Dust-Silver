using Godot;
using System;
using Game.Core;
using Godot.Collections; // This is key to access your SceneManager

public partial class PalletTownButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed +=()=> SceneManager.ChangeLevel(LevelName.pallet_town, 0, true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}
