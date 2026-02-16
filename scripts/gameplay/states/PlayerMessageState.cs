using Game.Core;
using Game.Utilities;
using Godot;
using System;


namespace Game.Gameplay;
public partial class PlayerMessageState : State
{
	public override void _Ready ()
	{
		Signals.Instance.MessageBoxOpen += (value) => {
			if (!value)
			{
				StateMachine.ChangeState("Roam");
			}
		};
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!MessageManager.Scrolling() && Input.IsActionJustReleased("use"))
		{
			// MessageManager.ScrollText();
			
		}
	}
}
