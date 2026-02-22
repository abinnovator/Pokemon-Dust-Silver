using Godot;
using System;
using Game.Utilities;

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
            
            if (_battleSM != null && _battleSM.Battle != null)
            {
                var battle = _battleSM.Battle;
                if (battle.OpponentHP <= 0)
                {
                    _battleSM.ChangeState("BattleEndState");
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
    }
}
