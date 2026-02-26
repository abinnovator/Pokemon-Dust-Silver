using Godot;
using System;
using System.Threading.Tasks;
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class TrainerEnemyTurnState : State
    {
        private TrainerBattleStateMachine _battleSM;

        public override void _Ready()
        {
            _battleSM = StateMachine as TrainerBattleStateMachine;
        }

        public override async void EnterState()
        {
            base.EnterState();
            
            if (_battleSM != null && _battleSM.Battle != null)
            {
                var battle = _battleSM.Battle;
                await battle.ExecuteEnemyTurnAsync();
            }
        }
    }
}
