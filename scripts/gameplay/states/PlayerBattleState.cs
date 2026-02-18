using Godot;
using System;
using Game.Utilities;
using Game.Core;

namespace Game.Gameplay;

public partial class PlayerBattleState : State
{
	public override void _Ready ()
	{
		Signals.Instance.BattleStart += () => {
			StateMachine.ChangeState("Battle");
		};
	}

}
