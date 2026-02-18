using Godot;
using System;
using Game.Utilities;
using Game.Core;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
    public partial class BattleStateMachine : StateMachine
    {
        public BattleMain Battle { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Battle = GetParentOrNull<BattleMain>();
            
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
