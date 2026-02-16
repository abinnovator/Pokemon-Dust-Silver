using Godot;
using Game.Core;
using Game.Gameplay;

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }
    
    // This holds the current progress in memory
    public PlayerSaveResource CurrentSave;

    private const string SavePath = "user://savegame.tres";

    public override void _Ready()
    {
        Instance = this;
    }

    public void CreateNewGame()
    {
        CurrentSave = new PlayerSaveResource();
        // Initialize defaults
        CurrentSave.StoryProgress = PlayerStoryState.NEW_GAME;
        CurrentSave.MapName = "res://Scenes/Maps/OakLab.tscn";
        CurrentSave.GlobalPosition = new Vector2(100, 200);
        GD.Print("New Game Resource Created!");
    }

    public void SaveToDisk()
    {
        if (CurrentSave == null) return;
        
        Error err = ResourceSaver.Save(CurrentSave, SavePath);
        if (err == Error.Ok)
            GD.Print("Saved successfully to " + SavePath);
    }

    public void LoadFromDisk()
    {
        if (FileAccess.FileExists(SavePath))
        {
            // 1. Load the Resource into memory
            CurrentSave = ResourceLoader.Load<PlayerSaveResource>(SavePath);
            GD.Print("Save Data Loaded from Disk!");

            // 2. Tell the SceneManager to jump to the saved level
            // We use the 'CurrentLevel' enum you defined in your resource
            SceneManager.ChangeLevel(CurrentSave.CurrentLevel, 0, true);

            // 3. Deferred Positioning
            // We wait for the scene to actually load before moving the player
            GetTree().Connect("node_added", Callable.From((Node node) => {
                if (node is Player player) // Check for your player class
                {
                    player.GlobalPosition = CurrentSave.GlobalPosition;
                    GD.Print($"Player restored to {CurrentSave.GlobalPosition}");
                }
            }), (uint)ConnectFlags.OneShot);
        }
    }
}