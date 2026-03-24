using Game.Utilities;
using Game.Core;
using Godot;
using System;

public partial class Follower : CharacterBody2D
{
	[Export] public StateMachine StateMachine;
	[Export] public AnimatedSprite2D Sprite;
	private readonly System.Collections.Generic.Dictionary<StarterChoice, SpriteFrames> _appearanceFrames = new()
	{
		{StarterChoice.BULBASAUR, GD.Load<SpriteFrames>("res://resources/spriteframes/bulbasaur.tres") },
		{ StarterChoice.CHARMANDER, GD.Load<SpriteFrames>("res://resources/spriteframes/charmander.tres") },
		{ StarterChoice.SQUIRTLE, GD.Load<SpriteFrames>("res://resources/spriteframes/squirtle.tres") },
	};
	

	public override void _Ready()
	{
		UpdateVisibility();
		StateMachine ??= GetNode<StateMachine>("StateMachine");
		if (StateMachine != null)
		{
			StateMachine.ChangeState(StateMachine.GetNode<State>("Roam"));
		}
		else
		{
			GD.PrintErr("Follower: StateMachine not found!");
		}
	}

	public override void _Process(double delta)
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		bool hasStarter = SaveManager.Instance?.CurrentSave?.ChosenStarter != null && 
				   SaveManager.Instance.CurrentSave.ChosenStarter != StarterChoice.NONE;
Visible = hasStarter;

// Don't disable processing - the follower needs to process to follow
// Just make it invisible when there's no starter
		if (!IsInsideTree()) return;

		try
		{
			Sprite ??= GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (Sprite == null) return;

			if (hasStarter && _appearanceFrames.TryGetValue(SaveManager.Instance.CurrentSave.ChosenStarter, out var spriteFrames))
			{
				Sprite.SpriteFrames = spriteFrames;
			}
			else
			{
				Sprite.SpriteFrames = null;
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Follower Appearance Error: {e.Message}");
		}
	}
}
