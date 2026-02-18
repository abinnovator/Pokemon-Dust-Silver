using Godot;
using Game.Core;
using Godot.Collections; // This is key to access your SceneManager

public partial class StartScreen : CanvasLayer
{
	private Button _newGameButton;

	public override void _Ready()
	{
		_newGameButton = GetNode<Button>("MarginContainer/VBoxContainer/NewGameButton");
		var _continueButton = GetNode<Button>("MarginContainer/VBoxContainer/ContinueButton");

		_newGameButton.Pressed += OnNewGamePressed;

		if (FileAccess.FileExists("user://savegame.tres"))
		{
			_continueButton.Visible = true;
			// Connect to a new local method instead of a one-liner
			_continueButton.Pressed += OnContinuePressed;
		}
		else
		{
			_continueButton.Visible = false;
		}
	}

	private void OnContinuePressed()
	{
		SaveManager.Instance.LoadFromDisk();
		QueueFree();
	}

	private void OnNewGamePressed()
	{
		// levelName: pallet_town
		// trigger: 0 (default)
		// spawn: true (to instantiate the player)
		// 1. Create the fresh resource
		PlayerSaveResource newGameData = new PlayerSaveResource();
		
		// 2. Set default starting values
		newGameData.StoryProgress = PlayerStoryState.NEW_GAME;
		newGameData.GlobalPosition = new Vector2(100, 200); // Your spawn point
		newGameData.CaughtPokemonList = new Array<string>();
		if (SaveManager.Instance == null)
		{
			GD.PrintErr("SaveManager not found! Check your Autoload settings.");
			return;
		}
		// 3. Hand it over to your Global Manager or SaveSystem
		SaveManager.Instance.CurrentSave = newGameData;
		SaveManager.Instance.SaveToDisk();
		SceneManager.ChangeLevel(LevelName.pallet_town_ashs_house_f1, 0, true);
		QueueFree();
	}
}
