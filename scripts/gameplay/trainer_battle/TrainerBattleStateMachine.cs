using Godot;
using System;
using Game.Utilities;
using Game.Core;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
    public partial class TrainerBattleStateMachine : StateMachine
    {
        public TrainerBattleMain Battle { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Battle = GetParentOrNull<TrainerBattleMain>();
            
            if (Battle == null)
            {
                Logger.Error("BattleStateMachine must be a child of BattleMain!");
            }
        }

        public void StartBattle()
        {
            ChangeState("BattleStartState");
        }
    }
}
