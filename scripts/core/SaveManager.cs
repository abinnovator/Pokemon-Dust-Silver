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
            CurrentSave = ResourceLoader.Load<PlayerSaveResource>(SavePath);
            GD.Print("Save Data Loaded from Disk!");

            SceneManager.ChangeLevel(CurrentSave.CurrentLevel, 0, true, CurrentSave.GlobalPosition);
            Game.Core.Logger.Info(CurrentSave.PartyDetails);

            GetTree().Connect("node_added", Callable.From((Node node) => {
                if (node is Player player)
                {
                    player.GlobalPosition = CurrentSave.GlobalPosition;
                    
                    var playerInput = player.GetNodeOrNull<PlayerInput>("PlayerInput");
                    if (playerInput != null)
                    {
                        playerInput.Direction = CurrentSave.FacingDirection;
                        playerInput.TargetPosition = Vector2.Zero; 
                    }
                    
                    var camera = player.GetNodeOrNull<Camera2D>("Camera2D");
                    camera?.ResetSmoothing();
                    
                    GD.Print($"Player restored to {CurrentSave.GlobalPosition} facing {CurrentSave.FacingDirection}");
                }
            }), (uint)ConnectFlags.OneShot);
        }
    }

    public async void QuitGame()
    {
        if (CurrentSave == null) return;

        var player = GameManager.GetPlayer();
        if (player != null)
        {
            CurrentSave.GlobalPosition = player.GlobalPosition;
            
            var playerInput = player.GetNodeOrNull<PlayerInput>("PlayerInput");
            if (playerInput != null)
            {
                CurrentSave.FacingDirection = playerInput.Direction;
            }
        }

        if (SceneManager.GetCurrentLevel() != null)
        {
            CurrentSave.CurrentLevel = SceneManager.GetCurrentLevel().LevelName;
        }

        SaveToDisk();
        GD.Print($"Game saved at {CurrentSave.GlobalPosition} facing {CurrentSave.FacingDirection}");

        if (SceneManager.Instance != null)
        {
            await SceneManager.Instance.FadeOut();
            
            SceneManager.Instance.ResetSession();

            var playerNode = GameManager.GetPlayer();
            if (playerNode != null)
            {
                playerNode.QueueFree();
            }

            var startScreen = GD.Load<PackedScene>("res://scenes/ui/StartScreen.tscn").Instantiate();
            GameManager.Instance.AddChild(startScreen);

            await SceneManager.Instance.FadeIn();
        }
    }
}