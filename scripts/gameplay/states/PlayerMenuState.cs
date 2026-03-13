using Game.Core;
using Game.Utilities;
using Godot;
using System;


namespace Game.Gameplay;
public partial class PlayerMenuState : State
{
	public override void _Ready ()
	{
		Signals.Instance.MenuOpen += (value) => {
			if (!value)
			{
				StateMachine.ChangeState("Roam");
			}
		};
	}
	
	public override void _Process(double delta)
	{
		
	}
}
