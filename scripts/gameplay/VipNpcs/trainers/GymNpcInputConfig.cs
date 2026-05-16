using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

[GlobalClass]
public partial class GymNpcInputConfig : Resource
{
    [ExportGroup("Movement")]
    [ExportSubgroup("Common")]
    [Export] public NpcMovementType NpcMovementType = NpcMovementType.Static;
    
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

    [ExportGroup("Gym Leader Logic")]
    [Export] public string LeaderName = "Brock";
    
    [Export] public Array<string> VictoryMessages = new() { "I took you for granted.", "Here is the Boulder Badge!" };
    [Export] public Array<string> DefeatMessages = new() { "My rock-hard willpower shall not be broken!" };

    [Export] public bool IsGymLeader = true;
    [Export] public Badge Badge = Badge.BOULDER;

    [ExportSubgroup("Battle Data")]
    [Export] public Dictionary<PokemonID, int> TrainerTeam = new(); 
    
    [ExportSubgroup("Triggers")]
    [Export] public PlayerStoryState EventTrigger;
    [ExportSubgroup("Visual")]
    [Export] public AtlasTexture TrainerAtlas;
    [Export] public LevelName LastPokemonCenter;
}