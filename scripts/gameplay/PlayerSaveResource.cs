using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;

public partial class PlayerSaveResource : Resource
{
    // Story & World State
    [Export] public PlayerStoryState StoryProgress;
    [Export] public Vector2 GlobalPosition;
    [Export] public string MapName;

    // Pokemon Specific Data
    [Export] public StarterChoice ChosenStarter;
    [Export] public Vector2 FacingDirection;

    // An array to store the names or IDs of every Pokemon the player has

    [Export] public Array<string> CaughtPokemonList = new Array<string>();

    // Detailed Party Data (for levels, HP, etc. later)
    [Export] public Dictionary PartyDetails = new Dictionary();

    [Export] public LevelName CurrentLevel;
    [Export]
    public Godot.Collections.Dictionary lastPosition = new Godot.Collections.Dictionary
    {
        { "posX", 0 },
        { "posY", 0 }
    };

    [Export] public Array<Badge> Badges = new Array<Badge>();

    [Export] public Array<PlayerStoryState> CompletedStoryProgress = new Array<PlayerStoryState>();
}
