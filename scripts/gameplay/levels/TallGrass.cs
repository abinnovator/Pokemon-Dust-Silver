using Game.Core;
using Godot;
using System;

namespace Game.Gameplay;

public partial class TallGrass : Area2D
{
	public PlayerSaveResource CurrentSave;

	private const string SavePath = "user://savegame.tres";
	[Export]
	public AnimatedSprite2D AnimatedSprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AnimatedSprite ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");	
		BodyEntered += onBodyEntered;
		BodyExited += onBodyExited;
	}
	public void onBodyEntered(Node2D node2d){
		var className = node2d.GetType().Name;
		switch (className){
			case "Player":
				CalculateEncounterChance();
				break;
		}
		AnimatedSprite.Play("down");
	}
	public void onBodyExited(Node2D node2d){
		AnimatedSprite.Play("up");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

	public async void CalculateEncounterChance()
	{
		var currentLevel = SceneManager.GetCurrentLevel();
		int rate = currentLevel.encounterRate;
		int chance = Globals.GetRandomNumberGenerator().RandiRange(0, 100);

		if (chance <= rate)
		{
			// 1. Find the FadeOverlay in your scene
			// Note: Make sure you have a ColorRect named "FadeOverlay" in a CanvasLayer
			var fade = GetTree().Root.FindChild("FadeOverlay", true, false) as ColorRect;

			if (fade != null)
			{
				// Fade to Black over 0.5 seconds
				Tween tween = CreateTween();
				tween.TweenProperty(fade, "color:a", 1.0f, 0.5f);
				await ToSignal(tween, "finished");
			}

			// 2. Determine the Wild Pokemon
			string[] possibleMons = currentLevel.wildPokemonList; 
			int index = Globals.GetRandomNumberGenerator().RandiRange(0, possibleMons.Length - 1);
			string encounteredMonName = possibleMons[index];

			// 3. Convert string to PokemonID Enum
			if (Enum.TryParse(encounteredMonName, true, out PokemonID id))
			{
				await MessageManager.PlayText($"A wild {encounteredMonName.ToUpper()} appeared!");

				// Load the player save data
				CurrentSave = ResourceLoader.Load<PlayerSaveResource>(SavePath);
				PokemonID playerId = (PokemonID)(int)CurrentSave.ChosenStarter;

				// 4. Use BattleManager to handle the wild battle transition and state saving
				BattleManager.Instance.StartWildBattle(id, playerId);
			}
			else
			{
				GD.PrintErr($"Could not find PokemonID for: {encounteredMonName}");
				
				// Emergency fade out if the pokemon load fails
				if (fade != null)
				{
					Tween tweenOut = CreateTween();
					tweenOut.TweenProperty(fade, "color:a", 0.0f, 0.2f);
				}
			}
		}
	}
}
