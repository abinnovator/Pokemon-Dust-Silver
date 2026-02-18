using Godot;
using System;
using Game.Utilities;
using Game.Core;

namespace Game.Gameplay
{
    public partial class CheckFaintState : State
    {
        private BattleStateMachine _battleSM;

        public override void _Ready()
        {
            _battleSM = StateMachine as BattleStateMachine;
        }

        public override void EnterState()
        {
            base.EnterState();
            
            // TODO: Actual HP check logic
            // For now, we'll just loop back to PlayerTurnState or end the battle
            // Let's assume the battle continues for now
            
            _battleSM.ChangeState("PlayerTurnState");
        }
    }
}
