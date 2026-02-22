using Godot;
using System;
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class PlayerTurnState : State
    {
        private BattleStateMachine _battleSM;

        public override void _Ready()
        {
            _battleSM = StateMachine as BattleStateMachine;
        }

        public override void EnterState()
        {
            base.EnterState();
            
            if (_battleSM != null && _battleSM.Battle != null)
            {
                var battle = _battleSM.Battle;
                battle.UpdateLog("What will you do?");
                if (battle.CommandMenu != null) battle.CommandMenu.Visible = true;
                if (battle.MoveMenu != null) battle.MoveMenu.Visible = false;
            }
        }
    }
}
