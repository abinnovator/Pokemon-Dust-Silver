using Godot;
using System;
using System.Threading.Tasks;
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class BattleStartState : State
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
                _battleSM.Battle.UpdateLog("A wild Pokemon appeared!");
                await Task.Delay(1000);
                _battleSM.ChangeState("PlayerTurnState");
            }
        }
    }
}
