using Godot;
using System;
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class TrainerCheckFaintState : State
    {
        private TrainerBattleStateMachine _battleSM;

        public override void _Ready()
        {
            _battleSM = StateMachine as TrainerBattleStateMachine;
        }

        public override void EnterState()
        {
            base.EnterState();
            
            if (_battleSM != null && _battleSM.Battle != null)
            {
                var battle = _battleSM.Battle;
                if (battle.OpponentHP <= 0)
                {
                    _ = HandleOpponentFaintedAsync(battle);
                }
                else if (battle.PlayerHP <= 0)
                {
                    _battleSM.ChangeState("BattleEndState");
                }
                else
                {
                    // If nobody fainted, switch turn
                    if (battle.LastTurnWasPlayer)
                    {
                        _battleSM.ChangeState("EnemyTurnState");
                    }
                    else
                    {
                        _battleSM.ChangeState("PlayerTurnState");
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task HandleOpponentFaintedAsync(TrainerBattleMain battle)
        {
            bool sentNext = await battle.TrySendNextEnemyPokemon();
            if (sentNext)
                _battleSM.ChangeState("PlayerTurnState");
            else
                _battleSM.ChangeState("BattleEndState");
        }
    }
}
