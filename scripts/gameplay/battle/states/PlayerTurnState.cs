using Godot;
using System;
using Game.Utilities;
using Game.Core;

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
            
            var battle = _battleSM.Battle;
            battle.UpdateLog("What will you do?");
            
            // Show the command menu
            if (battle.CommandMenu != null) battle.CommandMenu.Show();
            if (battle.MoveMenu != null) battle.MoveMenu.Hide();
        }

        public override void ExitState()
        {
            base.ExitState();
            var battle = _battleSM.Battle;
            if (battle.CommandMenu != null) battle.CommandMenu.Hide();
        }

        // Logic for move selection would be triggered by UI buttons calling into here or changing state
        public void OnMoveSelected(string moveName)
        {
            var battle = _battleSM.Battle;
            battle.UpdateLog($"Player used {moveName}!");
            _battleSM.ChangeState("EnemyTurnState");
        }
    }
}
