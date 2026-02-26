using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

[GlobalClass]
public partial class StoryNpcInputConfig : Resource
{
	[ExportGroup("Movement")]
	[ExportSubgroup("Common")]
	[Export]
	public NpcMovementType NpcMovementType = NpcMovementType.Static;

	private Array<string> _messages = new();
	[Export]
	public Array<string> Messages
	{
		get => _messages ??= new();
		set => _messages = value;
	}

	[ExportSubgroup("Wander")]
	[Export] public Vector2 WanderOrigin = Vector2.Zero;
	[Export] public double WanderRadius = 64f;
	[Export] public double WanderMoveInterval = 2f;

	[ExportSubgroup("Patrol")]
	private Array<Vector2> _patrolPoints = new();
	[Export]
	public Array<Vector2> PatrolPoints
	{
		get => _patrolPoints ??= new();
		set => _patrolPoints = value;
	}
	[Export] public double PatrolMoveInterval = 2f;
	[Export] public int PatrolIndex = 0;

	[ExportSubgroup("LookAround")]
	[Export] public double LookAroundInterval = 2f;
	[Export] public double LookAroundMoveInterval = 2f;

	[ExportGroup("Story Triggers")]
	[Export] public PlayerStoryState EventTrigger;

	[ExportGroup("Blocking")]
	[Export] public bool IsBlocker = false;
	[Export] public PlayerStoryState UnblockCondition;
	[Export] public Vector2 ClearPosition = Vector2.Zero;

	[ExportGroup("Forced Interception")]
	[Export] public bool HasForcedInterception = false;
	[Export] public Vector2 TriggerZoneSize = new Vector2(16, 48);
	[Export] public Vector2 TriggerZoneOffset = Vector2.Zero;

	private Array<string> _interceptMessages = new();
	[Export]
	public Array<string> InterceptMessages
	{
		get => _interceptMessages ??= new();
		set => _interceptMessages = value;
	}

	[ExportGroup("Battle")]
	[Export] public bool HasBattle = false;
	[Export] public PokemonID PokemonID = PokemonID.none;
	[Export] public int PokemonLevel = 5;
	[Export] public bool IsFemale = false;
	[Export] public bool IsMale = false;
	[Export] public bool IsShiny = false;
}
