using Godot;
using System;
using Logger = Game.Core.Logger;
using Game.Core;

namespace Game.Gameplay
{
	// Ensure this enum is defined correctly to match your logic
	public enum ECharacterAnimationState { 
		idle_up, idle_down, idle_left, idle_right, 
		walk_up, walk_down, walk_left, walk_right,
		turn_up, turn_down, turn_left, turn_right 
	}

	public partial class CharacterAnimation : AnimatedSprite2D
	{
		[ExportCategory("Nodes")]
		[Export] public CharacterMovement CharacterMovement;
		[Export] public CharecterInput CharacterInput;

		[ExportCategory("Animation vars")]
		[Export] public ECharacterAnimationState CurrentAnimationState = ECharacterAnimationState.idle_down;
		
		public override void _Ready()
		{
			CharacterMovement.Animation += PlayAnimation;

			Logger.Info("CharacterAnimation ready");
		}

		public void PlayAnimation(string animationType)
		{
			ECharacterAnimationState previousAnimation = CurrentAnimationState;

			if (CharacterMovement.IsMoving()) return;

			switch (animationType)
			{
				case "walk": // Fixed: Single quotes to Double quotes
					if (CharacterInput.Direction == Vector2.Up)
					{
						CurrentAnimationState = ECharacterAnimationState.walk_up;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						CurrentAnimationState = ECharacterAnimationState.walk_down;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						CurrentAnimationState = ECharacterAnimationState.walk_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						CurrentAnimationState = ECharacterAnimationState.walk_right;
					}
					break;

				case "idle": // Fixed: Single quotes to Double quotes
					if (CharacterInput.Direction == Vector2.Up)
					{
						CurrentAnimationState = ECharacterAnimationState.idle_up;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						CurrentAnimationState = ECharacterAnimationState.idle_down;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						CurrentAnimationState = ECharacterAnimationState.idle_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						CurrentAnimationState = ECharacterAnimationState.idle_right;
					}
					break;

				case "turn": // Fixed: Single quotes to Double quotes
					if (CharacterInput.Direction == Vector2.Up)
					{
						CurrentAnimationState = ECharacterAnimationState.turn_up;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						CurrentAnimationState = ECharacterAnimationState.turn_down;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						CurrentAnimationState = ECharacterAnimationState.turn_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						CurrentAnimationState = ECharacterAnimationState.turn_right;
					}
					break;
			}

			if (previousAnimation != CurrentAnimationState)
			{
				Play(CurrentAnimationState.ToString());
			}
		}
	}
}
