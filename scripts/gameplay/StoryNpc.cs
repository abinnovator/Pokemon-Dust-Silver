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
		{ StoryNpcAppearance.FatMan, GD.Load<SpriteFrames>("res://resources/spriteframes/FatMan.tres") },
		{StoryNpcAppearance.Jackson, GD.Load<SpriteFrames>("res://resources/spriteframes/Jackson.tres")},
		{StoryNpcAppearance.BugCatcher, GD.Load<SpriteFrames>("res://resources/spriteframes/bug_catcher.tres")},
		{StoryNpcAppearance.lass, GD.Load<SpriteFrames>("res://resources/spriteframes/lass.tres")},
		{StoryNpcAppearance.bird_keeper, GD.Load<SpriteFrames>("res://resources/spriteframes/bird_keeper.tres")}
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

		if (InputConfig is StoryNpcInputConfig config)
		{
			// Check if this is a shop clerk
			if (config.IsShopClerk)
			{
				// Show shop greeting
				string greeting = !string.IsNullOrEmpty(config.ShopGreeting) 
					? config.ShopGreeting 
					: "Welcome to my shop! Take a look at my wares.";
				
				await MessageManager.PlayText(greeting);
				
				// Open shop if items are available
				if (config.ShopItems.Count > 0 && ShopManager.Instance != null)
				{
					// Convert ItemResource array to ShopItem array
					var shopItems = new Godot.Collections.Array<ShopItem>();
					foreach (var itemResource in config.ShopItems)
					{
						var shopItem = new ShopItem
						{
							ItemName = itemResource.Name,
							ItemId = itemResource.Id,
							Description = itemResource.Description,
							Price = itemResource.Cost,
							Category = ParseItemCategory(itemResource.Category),
							Stock = -1, // Infinite stock by default
							IsKeyItem = itemResource.Category == "key-items",
							Icon = itemResource.Sprite
						};
						shopItems.Add(shopItem);
					}
					
					await ShopManager.Instance.OpenShop(shopItems, greeting);
				}
				else
				{
					await MessageManager.PlayText("Sorry, I'm out of stock right now.");
				}
				
				_stateMachine.ChangeState("Roam");
				return;
			}

			// Check if this trainer was already defeated
			if (config.HasBattle && !string.IsNullOrEmpty(config.TrainerID))
			{
				if (IsTrainerDefeated(config.TrainerID))
				{
					// Show after-battle message
					string message = !string.IsNullOrEmpty(config.AfterBattleMessage) 
						? config.AfterBattleMessage 
						: "You're pretty strong!";
					await MessageManager.PlayText(new[] { message });
					_stateMachine.ChangeState("Roam");
					return;
				}
			}

			// Check if story trigger requirement is met
			bool storyRequirementMet = CheckStoryRequirement(config);

			if (!storyRequirementMet)
			{
				// Show alternate message if story trigger not reached
				string message = !string.IsNullOrEmpty(config.StoryNotMetMessage) 
					? config.StoryNotMetMessage 
					: "I'm not ready to talk to you yet. Come back later!";
				await MessageManager.PlayText(new[] { message });
				_stateMachine.ChangeState("Roam");
				return;
			}

			// Show regular messages
			if (config.Messages.Count > 0)
			{
				await MessageManager.PlayText([.. config.Messages]);
			}

			// Check if this NPC has a battle
			if (config.HasBattle && config.PokemonID != PokemonID.none)
			{
				// Add a small delay to let messages fully close and player prepare
				await Task.Delay(500);
				
				// Initiate trainer battle
				_stateMachine.ChangeState("Roam");
				BattleManager.Instance.StartBattle(config);
				return;
			}
		}

		_stateMachine.ChangeState("Roam");
	}

	private bool IsTrainerDefeated(string trainerID)
	{
		if (SaveManager.Instance?.CurrentSave != null)
		{
			return SaveManager.Instance.CurrentSave.DefeatedTrainers.Contains(trainerID);
		}
		return false;
	}

	private bool CheckStoryRequirement(StoryNpcInputConfig config)
	{
		// If no specific event trigger is set, allow interaction
		if (config.EventTrigger == PlayerStoryState.NEW_GAME)
		{
			return true;
		}

		// Check if player has reached the required story state
		if (SaveManager.Instance?.CurrentSave != null)
		{
			var playerProgress = SaveManager.Instance.CurrentSave.StoryProgress;
			
			// Check if the player's story progress is >= the required trigger
			// This allows interactions once the requirement is met
			return playerProgress >= config.EventTrigger;
		}

		// If no save exists, default to allowing interaction
		return true;
	}

	public bool HasUnblocked
	{
		get => _hasUnblocked;
		set => _hasUnblocked = value;
	}

	public StoryNpcInput NpcInput => _npcInput;
	public StateMachine NpcStateMachine => _stateMachine;

	private ItemCategory ParseItemCategory(string category)
	{
		return category?.ToLower() switch
		{
			"pokeball" or "pokeballs" => ItemCategory.Pokeball,
			"medicine" => ItemCategory.Medicine,
			"battle-items" or "battleitem" => ItemCategory.BattleItem,
			"berries" or "berry" => ItemCategory.Berry,
			"key-items" or "keyitem" => ItemCategory.KeyItem,
			"machines" or "tm" => ItemCategory.TM,
			"mail" => ItemCategory.Mail,
			_ => ItemCategory.General
		};
	}
}
