using Godot;
using System;
using Logger = Game.Core.Logger;
using Game.Core;
using Godot.Collections;

namespace Game.Gameplay
{
	public partial class CharacterMovement : Node
	{
		// Signal definition - The "EventHandler" suffix is removed by Godot for the SignalName
		[Signal] public delegate void AnimationEventHandler(string animationType);

		[ExportCategory("Nodes")]
		[Export] public Node2D Character;

		[Export] public CharecterInput CharacterInput; 
		[Export] public CharacterCollisionRaycast CharacterCollisionRaycast;

		[ExportCategory("Movement Settings")] 
		[Export] public Vector2 TargetPosition = Vector2.Down;

		[Export] public bool IsWalking = false;
		public Vector2 StartPosition;

		[Export] public bool CollisionDetected = false;
		[Export]
		public bool IsJumping = false;
		[Export]
		public float JumpHeight = 10f;
		[Export]
		public float LerpSpeed = 1.2f;
		[Export]
		public float Progress = 0f;
		[Export]
		public ECharacterMovement ECharacterMovement = ECharacterMovement.WALKING;


		public override void _Ready()
		{
			// Connect to signals using the C# event syntax or method names
			CharacterInput.Walk += StartMoving;
			CharacterInput.Turn += Turn; // Fixed to match method name below


			Logger.Info("CharacterMovement ready");
		}

		public override void _Process(double delta)
		{
			Walk(delta);
			Jump(delta);

			if(!IsMoving())
			{
				if (GetParent().Name == "Player")
				{
					if (Modules.IsActionJustPressed())
					{
						return;
					}
				}
				
				EmitSignal(SignalName.Animation, "idle");
			}
		}

		public bool IsMoving() 
		{
			return IsWalking || IsJumping;
		}
		public bool IsColliding() 
		{
			return CollisionDetected;
		}

		public (Vector2, Array<Dictionary>) GetTargetColliders(Vector2 targetPosition)
		{
			var spaceState = GetViewport().GetWorld2D().DirectSpaceState;
			Vector2 adjustedTargetPosition = targetPosition;
			adjustedTargetPosition.X += 8;
			adjustedTargetPosition.Y += 8;
			
			var query = new PhysicsPointQueryParameters2D
			{
				Position = adjustedTargetPosition,
				CollisionMask = 1,
				CollideWithAreas = true
			};

			return (adjustedTargetPosition, spaceState.IntersectPoint(query));
		}


		private bool isTargetOccupied(Vector2 targetPosition)
		{
			var (adjustedTargetPosition, result) = GetTargetColliders(targetPosition);

			if (result.Count ==0){
				return false;
			}else if (result.Count ==1){

					var collider = (Node)(GodotObject)result[0]["collider"];
					var colliderType = collider.GetType().Name;
					Logger.Info($"Checking collision at {adjustedTargetPosition}. Hit: {collider.Name}, Type: {colliderType}");
					
					return colliderType switch
					{
						"Sign" => true,
						"TallGrass" => false,
						"TileMapLayer" => GetTileMapLayerCollision((TileMapLayer)collider,adjustedTargetPosition),
						"SceneTrigger"=> false,
						"BreakableTree" => true,
						_ => true,
					};
				
			}
			else
			{
				return true;
			}
		}
		public bool GetTileMapLayerCollision(TileMapLayer tileMapLayer,Vector2 adjustedTargetPosition)
		{
			Vector2I tileCordinates =tileMapLayer.LocalToMap(adjustedTargetPosition);
			TileData tileData = tileMapLayer.GetCellTileData(tileCordinates);

			if (tileData == null)
			{
				return true;
			}
			var ledgeDirection = (string)tileData.GetCustomData("LEDGE");
			if (ledgeDirection == null)
			{
				return true;
			}
			Logger.Info($"Ledge direction: {ledgeDirection}");
			switch (ledgeDirection)
			{
				case "DOWN":
				if (CharacterInput.Direction == Vector2.Down)
				{
					ECharacterMovement = ECharacterMovement.JUMPING;
					return false;
				}
				break;
				case "UP":
				if (CharacterInput.Direction == Vector2.Up)
				{
					ECharacterMovement = ECharacterMovement.JUMPING;
					return false;
				}
				break;
				case "RIGHT":
				if (CharacterInput.Direction == Vector2.Right)
				{
					ECharacterMovement = ECharacterMovement.JUMPING;
					return false;
				}
				break;
				case "LEFT":
				if (CharacterInput.Direction == Vector2.Left)
				{
					ECharacterMovement = ECharacterMovement.JUMPING;
					return false;
				}
				break;
				
			}
			return true;
		}
		public void StartMoving()
		{
			if (SceneManager.isChanging)
			{
				return;
			}
			// Fix: Update TargetPosition from input
			if (CharacterInput.TargetPosition != Vector2.Zero)
			{
				TargetPosition = Character.Position + CharacterInput.TargetPosition;
			}
			
			if (!IsMoving() && !isTargetOccupied(TargetPosition) && SceneManager.GetCurrentLevel().ReservedTile(TargetPosition))
			{
				EmitSignal(SignalName.Animation, "walk");
				Logger.Info($"Moving from {Character.Position} to {TargetPosition}");
				

				if (ECharacterMovement == ECharacterMovement.JUMPING)
				{
					Progress = 0f;
					StartPosition = Character.Position;
					TargetPosition = Character.Position + (CharacterInput.TargetPosition * 2);
					IsJumping = true;
				}
				else {
					IsWalking = true;
				}
			}
		}

		public void Walk(double delta)
		{
			if (IsWalking)
			{
				float moveSpeed = (float)delta * Globals.GridSize * 4;
				Character.Position = Character.Position.MoveToward(TargetPosition, moveSpeed);

				if (Character.Position.DistanceTo(TargetPosition) < 0.1f)
				{
					StopWalking();
				}
			}
		}

		public void Turn()
		{
			EmitSignal(SignalName.Animation, "turn");
		}
		public void Jump(double delta)
		{
			if (IsJumping)
			{
				Progress += LerpSpeed * (float)delta;
				Vector2 position = StartPosition.Lerp(TargetPosition, Progress);
				float parabolicOffset = JumpHeight * 4 * Progress * (1 - Progress);
				position.Y -= parabolicOffset;
				Character.Position = position;

				if (Progress >= 1.0f)
				{
					StopWalking();
				}
			}
		}

		public void StopWalking()
		{
			SceneManager.GetCurrentLevel().ReleaseTile(TargetPosition);
			IsWalking = false;
			IsJumping = false;
			ECharacterMovement = ECharacterMovement.WALKING;
			SnapPositionToGrid();
			EmitSignal(SignalName.Animation, "idle");
		}

		public void SnapPositionToGrid()
		{
			int gridSize = Globals.GridSize;
			float snappedX = Mathf.Round(Character.Position.X / gridSize) * gridSize;
			float snappedY = Mathf.Round(Character.Position.Y / gridSize) * gridSize;
			Character.Position = new Vector2(snappedX, snappedY);
		}
	}
}
