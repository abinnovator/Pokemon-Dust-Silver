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
	private AudioStreamPlayer _audioStreamPlayer;
	public override void _Ready()
	{
		 _audioStreamPlayer = GetNode<AudioStreamPlayer>("Audio");
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
		_audioStreamPlayer.Play();
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

			string[] possibleMons = currentLevel.wildPokemonList; 
			int index = Globals.GetRandomNumberGenerator().RandiRange(0, possibleMons.Length - 1);
			string encounteredMonName = possibleMons[index];

			if (Enum.TryParse(encounteredMonName, true, out PokemonID id))
			{
				await MessageManager.PlayText($"A wild {encounteredMonName.ToUpper()} appeared!");

				global::Game.Core.BattleManager.Instance.StartWildBattle(id, global::Game.Core.PokemonID.none);
			}
			else
			{
				GD.PrintErr($"Could not find PokemonID for: {encounteredMonName}");
				
				if (fade != null)
				{
					Tween tweenOut = CreateTween();
					tweenOut.TweenProperty(fade, "color:a", 0.0f, 0.2f);
				}
			}
		}
	}
}
