using Game.Core;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System;
using Logger = Game.Core.Logger;
using System.Threading.Tasks;


namespace Game.Gameplay;
[Tool]
public partial class Computer : StaticBody2D
{
	
	public override void _Ready()
	{
		
	}
	private bool _isInteracting = false; 
	public async void openBox (){
		var boxScene = GD.Load<PackedScene>("res://scenes/core/trainer_battle_ui.tscn").Instantiate();
		GetTree().Root.AddChild(boxScene);

	}
	
}
