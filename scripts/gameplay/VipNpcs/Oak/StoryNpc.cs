using System;
using System.Collections.Generic;
using Game.Core;
using Game.Utilities;
using Godot;
using Godot.Collections;
using Logger = Game.Core.Logger;
using System.Linq;

namespace Game.Gameplay;

[Tool]
public partial class OakNpc : CharacterBody2D
{
	private StoryNpcAppearance _npcAppearance = StoryNpcAppearance.OldMan;

	[ExportCategory("Traits")]
	[Export]
	public StoryNpcAppearance NpcAppearance
	{
		get => _npcAppearance;
		set
		{
			if (_npcAppearance != value)
			{
				_npcAppearance = value;
				UpdateAppearence();
			}
		}
	}

	// Using base 'Resource' here bypasses the Godot 'InvalidCastException' 
	// that happens when the Engine and C# types get out of sync.
	[Export]
	public Resource InputConfig { get; set; }

	private AnimatedSprite2D _animatedSprite2D;
	public StoryNpcInput _npcInput;
	public StateMachine _stateMachine;
	private CharacterMovement _characterMovement;

	private readonly System.Collections.Generic.Dictionary<StoryNpcAppearance, SpriteFrames> _appearanceFrames = new()
	{
		{ StoryNpcAppearance.Delia, GD.Load<SpriteFrames>("res://resources/spriteframes/Delia.tres") },
		{ StoryNpcAppearance.ProfessorOak, GD.Load<SpriteFrames>("res://resources/spriteframes/Oak.tres") },
	};

	public override void _Ready()
	{
		UpdateAppearence();

		if (Engine.IsEditorHint()) return;

		_npcInput = GetNode<StoryNpcInput>("Input");
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_characterMovement = GetNode<CharacterMovement>("Movement");
		_stateMachine = GetNode<StateMachine>("StateMachine");

		// MANUAL CAST: This is safer and prevents the crash you're seeing.
		if (InputConfig is StoryNpcInputConfig config)
		{
			_npcInput.NpcInputConfig = config;
		}
		else if (InputConfig != null)
		{
			GD.PrintErr($"{Name}: InputConfig is the wrong type! Expected StoryNpcInputConfig.");
		}
		else
		{
			GD.PrintErr($"{Name}: InputConfig is missing in the Inspector!");
		}

		_stateMachine.ChangeState("Roam");
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint()) return;

		var player = GameManager.GetPlayer();
		if (player != null)
		{
			ZIndex = (player.Position.Y <= Position.Y) ? 6 : 4;
		}
	}

	private void UpdateAppearence()
	{
		if (!IsInsideTree()) return;

		try
		{
			_animatedSprite2D ??= GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (_animatedSprite2D == null) return;

			if (_appearanceFrames.TryGetValue(_npcAppearance, out var spriteFrames))
			{
				_animatedSprite2D.SpriteFrames = spriteFrames;
			}
			else
			{
				_animatedSprite2D.SpriteFrames = null;
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"StoryNpc Appearance Error: {e.Message}");
		}
	}

	public async System.Threading.Tasks.Task PlayMessage(Vector2 playerDirection)
	{
		if (Engine.IsEditorHint()) return;
		if (_characterMovement.IsMoving()) return;

		if (_npcInput.Direction != playerDirection * -1)
		{
			_npcInput.Direction = playerDirection * -1;
			_npcInput.EmitSignal(CharecterInput.SignalName.Turn);
		}
		GD.Print("NPC: Attempting to play message...");

		_stateMachine.ChangeState("Message");

		// Cast check before playing messages
		if (InputConfig is StoryNpcInputConfig config && config.Messages.Count > 0)
		{
			await MessageManager.PlayText(null, config.Messages.ToArray());
		}
		
		_stateMachine.ChangeState("Roam");
	}

	public async System.Threading.Tasks.Task MoveToPosition(Vector2 targetWorldPosition)
	{
		if (Engine.IsEditorHint()) return;
		if (_characterMovement == null || _npcInput == null) return;

		// Wait for any current movement to finish
		while (_characterMovement.IsMoving())
		{
			await System.Threading.Tasks.Task.Delay(50);
		}

		// Calculate path to target
		Vector2 currentPos = GlobalPosition;
		Vector2 direction = (targetWorldPosition - currentPos).Normalized();

		// Move in grid steps until we reach the target
		while (currentPos.DistanceTo(targetWorldPosition) > Globals.GridSize / 2)
		{
			// Determine the primary movement direction
			Vector2 moveDir = Vector2.Zero;
			float deltaX = targetWorldPosition.X - currentPos.X;
			float deltaY = targetWorldPosition.Y - currentPos.Y;

			if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
			{
				moveDir = deltaX > 0 ? Vector2.Right : Vector2.Left;
			}
			else
			{
				moveDir = deltaY > 0 ? Vector2.Down : Vector2.Up;
			}

			// Set input direction and target
			_npcInput.Direction = moveDir;
			_npcInput.TargetPosition = moveDir * Globals.GridSize;
			_npcInput.EmitSignal(CharecterInput.SignalName.Walk);

			// Wait for movement to complete
			await System.Threading.Tasks.Task.Delay(50);
			while (_characterMovement.IsMoving())
			{
				await System.Threading.Tasks.Task.Delay(50);
			}

			currentPos = GlobalPosition;
		}
	}

	public async System.Threading.Tasks.Task FaceDirection(Vector2 direction)
	{
		if (Engine.IsEditorHint()) return;
		if (_npcInput == null) return;

		_npcInput.Direction = direction;
		_npcInput.EmitSignal(CharecterInput.SignalName.Turn);
		await System.Threading.Tasks.Task.Delay(100);
	}
}
