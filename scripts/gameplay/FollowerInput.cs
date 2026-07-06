using Godot;
using System;
using Game.Core;
using Game.Gameplay.States;

namespace Game.Gameplay
{
	public partial class FollowerInput : CharecterInput
	{
		[ExportCategory("Follower")]
		[Export] public Node2D PlayerNode;
		[Export] public Node2D FollowerNode;
		[Export] public float FollowDistance = 32f; // 2 grid tiles (16 * 2)
		
		private Vector2 lastPlayerPosition;

		public override void _Ready()
		{
			if (PlayerNode != null)
			{
				lastPlayerPosition = PlayerNode.GlobalPosition;
			}
			// Game.Core.Logger.Info($"FollowerInput ready. PlayerNode: {PlayerNode?.Name ?? "null"}, FollowerNode: {FollowerNode?.Name ?? "null"}");
		}

		public override void _Process(double delta)
		{
			if (!HasStarter()) return;
			if (PlayerNode == null || FollowerNode == null) return;
			
			UpdateFollowDirection();
		}

		private bool HasStarter()
		{
			return SaveManager.Instance?.CurrentSave?.ChosenStarter != null && 
       		SaveManager.Instance.CurrentSave.ChosenStarter != StarterChoice.NONE;
		}

		private void UpdateFollowDirection()
		{
			Vector2 currentPlayerPos = PlayerNode.GlobalPosition;
			Vector2 distanceToPlayer = currentPlayerPos - FollowerNode.GlobalPosition;
			float distance = distanceToPlayer.Length();

			// Only move if the player is far enough away
			if (distance > FollowDistance)
			{
				// Determine which direction to move based on the largest distance component
				Vector2 absDistance = distanceToPlayer.Abs();
				
				if (absDistance.X > absDistance.Y)
				{
					// Move horizontally
					if (distanceToPlayer.X > 0)
					{
						Direction = Vector2.Right;
						TargetPosition = new Vector2(Globals.GridSize, 0);
					}
					else
					{
						Direction = Vector2.Left;
						TargetPosition = new Vector2(-Globals.GridSize, 0);
					}
				}
				else
				{
					// Move vertically
					if (distanceToPlayer.Y > 0)
					{
						Direction = Vector2.Down;
						TargetPosition = new Vector2(0, Globals.GridSize);
					}
					else
					{
						Direction = Vector2.Up;
						TargetPosition = new Vector2(0, -Globals.GridSize);
					}
				}
			}
			else
			{
				// Close enough, face the player's direction
				if (PlayerNode is Player player && player.StateMachine != null)
				{
					var playerRoamState = player.StateMachine.GetNodeOrNull<PlayerRoamState>("Roam");
					if (playerRoamState != null && playerRoamState.PlayerInput != null)
					{
						Direction = playerRoamState.PlayerInput.Direction;
					}
				}
			}

			lastPlayerPosition = currentPlayerPos;
		}
	}
}
