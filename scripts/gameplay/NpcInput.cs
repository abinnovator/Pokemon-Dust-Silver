using Godot;
using System;

namespace Game.Gameplay;

public partial class NpcInput : CharecterInput
{
	[Export]
	public NpcInputConfig NpcInputConfig;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
