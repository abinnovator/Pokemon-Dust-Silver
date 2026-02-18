using Godot;
using System;
using Game.Utilities;
using Game.Core;

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
                Game.Core.Logger.Error("BattleStateMachine must be a child of BattleMain!");
            }
        }

        public void StartBattle()
        {
            ChangeState("BattleStartState");
        }
    }
}
