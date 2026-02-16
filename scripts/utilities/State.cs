using Godot;
using System;
using Logger = Game.Core.Logger;


namespace Game.Utilities
{
	public abstract partial class State : Node
	{
		[Export] public Node StateOwner;
        [Export] public StateMachine StateMachine;

        public virtual void EnterState() {
            Logger.Info($"Entering {this.GetType().Name}");
        }
        public virtual void ExitState() {
            Logger.Info($"Exiting {this.GetType().Name}");
        }
    }
}