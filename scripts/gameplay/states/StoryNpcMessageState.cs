using Godot;
using System;
using Game.Utilities;
using Game.Core;

namespace Game.Gameplay;

public partial class StoryNpcMessageState : State
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

}
