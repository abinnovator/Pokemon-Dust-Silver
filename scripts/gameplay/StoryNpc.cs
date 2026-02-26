using System;
using System.Threading.Tasks;
using Game.Core;
using Game.Utilities;
using Godot;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

[Tool]
public partial class StoryNpc : CharacterBody2D
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

	[Export]
	public Resource InputConfig { get; set; }

	private AnimatedSprite2D _animatedSprite2D;
	private StoryNpcInput _npcInput;
	private StateMachine _stateMachine;
	private CharacterMovement _characterMovement;
	private Area2D _triggerZone;
	private bool _hasIntercepted = false;
	private bool _hasUnblocked = false;

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

		if (InputConfig is StoryNpcInputConfig config)
		{
			_npcInput.NpcInputConfig = config;

			if (config.HasForcedInterception)
			{
				CreateTriggerZone(config);
			}
		}
		else if (InputConfig != null)
		{
			GD.PrintErr($"{Name}: InputConfig is the wrong type! Expected StoryNpcInputConfig.");
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

	private void CreateTriggerZone(StoryNpcInputConfig config)
	{
		_triggerZone = new Area2D();
		_triggerZone.CollisionLayer = 0;
		_triggerZone.CollisionMask = 1;
		_triggerZone.Monitorable = false;

		var shape = new CollisionShape2D();
		var rect = new RectangleShape2D();
		rect.Size = config.TriggerZoneSize;
		shape.Shape = rect;
		shape.Position = config.TriggerZoneOffset + new Vector2(8, 8);

		_triggerZone.AddChild(shape);
		AddChild(_triggerZone);

		_triggerZone.BodyEntered += OnTriggerZoneBodyEntered;
	}

	private void OnTriggerZoneBodyEntered(Node2D body)
	{
		if (_hasIntercepted) return;
		if (body is not Player) return;
		if (!(InputConfig is StoryNpcInputConfig)) return;

		_hasIntercepted = true;
		_stateMachine.ChangeState("Intercept");
	}

	public async Task TweenToGridPosition(Vector2 targetWorldPosition)
	{
		Vector2 diff = targetWorldPosition - Position;
		Vector2 direction;
		if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
			direction = diff.X > 0 ? Vector2.Right : Vector2.Left;
		else
			direction = diff.Y > 0 ? Vector2.Down : Vector2.Up;

		_npcInput.Direction = direction;
		_npcInput.TargetPosition = direction * Globals.GridSize;
		_npcInput.EmitSignal(CharecterInput.SignalName.Walk);

		var tween = CreateTween();
		tween.TweenProperty(this, "position", targetWorldPosition, 0.25f);
		await ToSignal(tween, "finished");

		SnapToGrid();
	}

	public async Task TweenToClearPosition(Vector2 clearPosition)
	{
		Vector2 current = Position;
		while (current.DistanceTo(clearPosition) >= Globals.GridSize)
		{
			Vector2 diff = clearPosition - current;
			Vector2 step;
			if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
				step = new Vector2(diff.X > 0 ? Globals.GridSize : -Globals.GridSize, 0);
			else
				step = new Vector2(0, diff.Y > 0 ? Globals.GridSize : -Globals.GridSize);

			Vector2 nextPos = current + step;
			await TweenToGridPosition(nextPos);
			current = Position;
		}

		if (current != clearPosition)
		{
			await TweenToGridPosition(clearPosition);
		}
	}

	public async Task WalkToPlayer()
	{
		var player = GameManager.GetPlayer();
		if (player == null) return;

		Vector2 playerPos = player.Position;
		Vector2 npcPos = Position;

		Vector2 diff = playerPos - npcPos;
		Vector2 adjacentPos;
		if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
			adjacentPos = playerPos + (diff.X > 0 ? Vector2.Left : Vector2.Right) * Globals.GridSize;
		else
			adjacentPos = playerPos + (diff.Y > 0 ? Vector2.Up : Vector2.Down) * Globals.GridSize;

		while (Position.DistanceTo(adjacentPos) >= Globals.GridSize)
		{
			Vector2 stepDiff = adjacentPos - Position;
			Vector2 step;
			if (Mathf.Abs(stepDiff.X) > Mathf.Abs(stepDiff.Y))
				step = new Vector2(stepDiff.X > 0 ? Globals.GridSize : -Globals.GridSize, 0);
			else
				step = new Vector2(0, stepDiff.Y > 0 ? Globals.GridSize : -Globals.GridSize);

			await TweenToGridPosition(Position + step);
		}

		if (Position.DistanceTo(adjacentPos) > 0.5f)
		{
			await TweenToGridPosition(adjacentPos);
		}
	}

	public void FaceToward(Vector2 targetPosition)
	{
		Vector2 diff = targetPosition - Position;
		Vector2 direction;
		if (Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
			direction = diff.X > 0 ? Vector2.Right : Vector2.Left;
		else
			direction = diff.Y > 0 ? Vector2.Down : Vector2.Up;

		_npcInput.Direction = direction;
		_npcInput.EmitSignal(CharecterInput.SignalName.Turn);
	}

	public void SnapToGrid()
	{
		int gridSize = Globals.GridSize;
		float snappedX = Mathf.Round(Position.X / gridSize) * gridSize;
		float snappedY = Mathf.Round(Position.Y / gridSize) * gridSize;
		Position = new Vector2(snappedX, snappedY);
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

	public async Task PlayMessage(Vector2 playerDirection)
	{
		if (Engine.IsEditorHint()) return;
		if (_characterMovement.IsMoving()) return;

		if (_npcInput.Direction != playerDirection * -1)
		{
			_npcInput.Direction = playerDirection * -1;
			_npcInput.EmitSignal(CharecterInput.SignalName.Turn);
		}

		_stateMachine.ChangeState("Message");

		if (InputConfig is StoryNpcInputConfig config && config.Messages.Count > 0)
		{
			await MessageManager.PlayText([.. config.Messages]);
		}

		_stateMachine.ChangeState("Roam");
	}

	public bool HasUnblocked
	{
		get => _hasUnblocked;
		set => _hasUnblocked = value;
	}

	public StoryNpcInput NpcInput => _npcInput;
	public StateMachine NpcStateMachine => _stateMachine;
}
