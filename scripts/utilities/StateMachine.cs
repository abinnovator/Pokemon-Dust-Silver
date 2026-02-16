using Godot;
using System;
using Logger = Game.Core.Logger;

namespace Game.Utilities
{
	public partial class StateMachine : Node
	{
		[ExportCategory("State Machine vars")]
		[Export] public Node Customer;
		[Export] public State CurrentState;

		public override void _Ready()
		{
			foreach (Node child in GetChildren())
			{
				if (child is State s) // Renamed to 's' to avoid confusion
				{
					s.StateOwner = Customer;
					s.SetProcess(false);
				}
			}
		}

		public string GetCurrentState() {
			return CurrentState?.Name.ToString() ?? "Null";
		}

		public void ChangeState(State newState) { // Changed parameter name to newState
			CurrentState?.ExitState();
			CurrentState = newState;
			CurrentState?.EnterState();

			foreach (Node child in GetChildren())
			{
				if (child is State childState) // Fixed: Variable name is now unique
				{
					childState.SetProcess(childState == CurrentState);
				}
			}
			
		}
		public void ChangeState(string newState) { // Changed parameter name to newState
			var _stats = GetNode<State>(newState);
			CurrentState?.ExitState();
			CurrentState = _stats;
			CurrentState?.EnterState();

			foreach (Node child in GetChildren())
			{
				if (child is State childState) // Fixed: Variable name is now unique
				{
					childState.SetProcess(childState == CurrentState);
				}
			}
			
		}
	}
}
