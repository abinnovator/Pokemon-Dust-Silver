using Godot;
using System;
using System.Threading.Tasks;
using Game.Utilities;
using Game.Core;

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
            
            var battle = _battleSM.Battle;
            battle.UpdateLog($"A wild {battle.OpponentID} appeared!");
            
            // Introduce a small delay for the "Intro"
            await Task.Delay(2000);
            
            _battleSM.ChangeState("PlayerTurnState");
        }
    }
}
