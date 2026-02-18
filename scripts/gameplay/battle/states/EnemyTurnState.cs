using Godot;
using System;
using System.Threading.Tasks;
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class EnemyTurnState : State
    {
        private BattleStateMachine _battleSM;

        public override void _Ready()
        {
            _battleSM = StateMachine as BattleStateMachine;
        }

        public override async void EnterState()
        {
            base.EnterState();
            
            if (_battleSM != null && _battleSM.Battle != null)
            {
                var battle = _battleSM.Battle;
                battle.UpdateLog($"Enemy {battle.OpponentID} is thinking...");
                
                await Task.Delay(1500);
                battle.UpdateLog($"Enemy {battle.OpponentID} used Tackle!");
                
                await Task.Delay(1000);
                _battleSM.ChangeState("CheckFaintState");
            }
        }
    }
}
