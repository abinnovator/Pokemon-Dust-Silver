using Game.Core;
using Game.Utilities;
using Godot;
using System;

namespace Game.Gameplay.States
{
	public partial class FollowerRoamState : State
	{
		[ExportCategory("State vars")]
		[Export] public FollowerInput FollowerInput;
		[Export] public CharacterMovement CharacterMovement;
		[Export] public float MoveInterval = 0.5f; // Time between movement attempts
		
		private float moveTimer = 0f;

		public override void _Ready()
		{
			moveTimer = MoveInterval;
			Game.Core.Logger.Info($"FollowerRoamState ready. FollowerInput: {FollowerInput?.Name ?? "null"}, CharacterMovement: {CharacterMovement?.Name ?? "null"}");
		}

		public override void _Process(double delta)
		{
			// Only process if the player has a starter Pokémon
			if (!HasStarter()) return;
			if (GameManager.IsPlayerMovementLocked) return;
			
			moveTimer -= (float)delta;
			
			if (moveTimer <= 0f && !CharacterMovement.IsMoving())
			{
				TryFollowPlayer();
				moveTimer = MoveInterval;
			}
		}

		private bool HasStarter()
		{
			return SaveManager.Instance?.CurrentSave?.ChosenStarter != null && 
       SaveManager.Instance.CurrentSave.ChosenStarter != StarterChoice.NONE;
		}

		private void TryFollowPlayer()
		{
			Game.Core.Logger.Info($"TryFollowPlayer called. FollowerInput null: {FollowerInput == null}, PlayerNode null: {FollowerInput?.PlayerNode == null}");
			if (FollowerInput == null || FollowerInput.PlayerNode == null) return;
			
			Vector2 distanceToPlayer = FollowerInput.PlayerNode.GlobalPosition - FollowerInput.FollowerNode.GlobalPosition;
			float distance = distanceToPlayer.Length();
			
			// Only move if far enough from player
			Game.Core.Logger.Info($"Follower trying to move. Distance: {distance}, FollowDistance: {FollowerInput.FollowDistance}");
			if (distance > FollowerInput.FollowDistance)
			{
				FollowerInput.EmitSignal(CharecterInput.SignalName.Walk);
			}
			else
			{
				// Turn to face the player's direction when close
				FollowerInput.EmitSignal(CharecterInput.SignalName.Turn);
			}
		}
	}
}
