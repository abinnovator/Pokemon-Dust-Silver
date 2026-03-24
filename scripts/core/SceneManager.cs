using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Gameplay;
using System.Collections.Generic;
using Godot.Collections;
using Game.Core;

namespace Game.Core
{
	public partial class SceneManager : Node
	{
		public static SceneManager Instance {get; private set;}
		public static bool isChanging {get; private set;}
		
		private static Follower followerInstance;
		
		[ExportCategory("Scene Manager Vars")]
		[Export]

		public ColorRect FadeRect {get; private set;}
		[Export]
		public Level CurrentLevel;
		[Export]
		public Array<Level> AllLevels;
		public override void _Ready()
		{
			Instance = this;
			isChanging = false;
			
			if (AllLevels == null)
			{
				AllLevels = new Array<Level>();
			}

			// Fix: Create FadeRect programmatically if null (because autoload is a script, not a scene)
			if (FadeRect == null)
			{
				var canvasLayer = new CanvasLayer();
				canvasLayer.Layer = 100; // Ensure it's on top
				AddChild(canvasLayer);
				
				FadeRect = new ColorRect();
				FadeRect.Color = new Color(0, 0, 0, 0); // Transparent black
				FadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
				FadeRect.MouseFilter = Control.MouseFilterEnum.Ignore; // Fix: Allow mouse input to pass through
				canvasLayer.AddChild(FadeRect);
			}

			Game.Core.Logger.Info("Scene Manager Ready");
		}

		public static async void ChangeLevel(LevelName levelName = LevelName.pallet_town, int trigger = 0, bool spawn = false, Vector2? customPosition = null)
		{
			Game.Core.Logger.Info($"ChangeLevel called for {levelName}. isChanging: {isChanging}");
			while (isChanging){
				await Instance.ToSignal(Instance.GetTree(), "process_frame");
			}
			isChanging = true;
			
			try
			{
				
				await Instance.GetLevel(levelName);
				await Instance.ToSignal(Instance.GetTree(), "process_frame");
				if (spawn)
				{
					Instance.Spawn(customPosition);
				}
				else
				{
					Instance.Switch(trigger);
				}
				await Instance.FadeIn();
			}
			catch (Exception e)
			{
				Game.Core.Logger.Error($"Error changing level: {e.Message}\n{e.StackTrace}");
			}
			finally
			{
				isChanging = false;
				Game.Core.Logger.Info("ChangeLevel finished, isChanging set to false");
			}
		}

		public async Task GetLevel(LevelName levelName)
		{
			Game.Core.Logger.Info($"Getting Level: {levelName}");
			if (AllLevels == null)
			{
				Game.Core.Logger.Warning("AllLevels list was null, initializing new list.");
				AllLevels = new Array<Level>();
			}

			if (IsInstanceValid(CurrentLevel))
			{
				await Instance.FadeOut();
				CurrentLevel.GetParent()?.RemoveChild(CurrentLevel);
			}
			
			// Filter out nulls and find level
			CurrentLevel = AllLevels.Where(l => l != null).FirstOrDefault(level => level.LevelName == levelName);
			if (CurrentLevel !=null)
			{
				GameManager.GetGameViewPort().AddChild(CurrentLevel);
			}
			else {
				string scenePath = "res://scenes/levels/" + levelName.ToString() + ".tscn";
				if (levelName == LevelName.battle_scene)
				{
					scenePath = "res://scenes/core/battle_ui.tscn";
					Game.Core.Logger.Info($"Loading battle scene from: {scenePath}");
					
					var battleSceneResource = GD.Load<PackedScene>(scenePath);
					if (battleSceneResource == null)
					{
						Game.Core.Logger.Error($"Failed to load battle scene at path: {scenePath}");
						return;
					}
					
					var battleScene = battleSceneResource.Instantiate();
					if (battleScene == null)
					{
						Game.Core.Logger.Error($"Failed to instantiate battle scene from: {scenePath}");
						return;
					}
					
					GameManager.GetGameViewPort().AddChild(battleScene);
					// Note: CurrentLevel remains as the previous level since battle is an overlay
					return;
				}
				Game.Core.Logger.Info($"Level not found in AllLevels, attempting to load from resource: {scenePath}");
				
				var sceneResource = GD.Load<PackedScene>(scenePath);
				if (sceneResource == null)
				{
					Game.Core.Logger.Error($"Failed to load level scene at path: {scenePath}. Check if file exists and name matches enum.");
					return;
				}

				try
{
CurrentLevel = sceneResource.Instantiate<Level>();
}
catch (System.InvalidCastException ex)
{
Game.Core.Logger.Error($"Failed to cast scene at {scenePath} to Level type. The scene root node must inherit from Level (Node2D). Error: {ex.Message}");
return;
}
				if (CurrentLevel == null)
				{
					Game.Core.Logger.Error($"Failed to instantiate level from scene: {scenePath}");
					return;
				}

				AllLevels.Add(CurrentLevel);
				GameManager.GetGameViewPort().AddChild(CurrentLevel);
			}

		}
		public void Spawn (Vector2? customPosition = null)
		{
			if (customPosition.HasValue)
			{
				var playerNode = GD.Load<PackedScene>("res://scenes/charecters/player.tscn").Instantiate<Player>();
				GameManager.AddPlayer(playerNode);
				playerNode.GlobalPosition = customPosition.Value;
SpawnFollower(playerNode, customPosition.Value);
return;
			}

			var spawnPoints = CurrentLevel.GetTree().GetNodesInGroup(LevelGroups.SPAWNPOINTS.ToString());
			if (spawnPoints.Count <= 0){
				throw new Exception("No spawn points found in level"); 
				
			}
			// Fix 2 & 3: Cast to Node2D (or SpawnPoint if that class exists) to access .Position
			var spawnPoint = (Node2D)spawnPoints[0];
			var player = GD.Load<PackedScene>("res://scenes/charecters/player.tscn").Instantiate<Player>();
 
			GameManager.AddPlayer(player);
			GameManager.GetPlayer().GlobalPosition = spawnPoint.GlobalPosition;
SpawnFollower(player, spawnPoint.GlobalPosition);
}
 
		public void Switch(int triggerId)
		{
			// 1. Get all triggers in the new scene
			var sceneTriggers = CurrentLevel.GetTree().GetNodesInGroup(LevelGroups.SCENETRIGGERS.ToString());
			
			if (sceneTriggers.Count <= 0)
			{
				throw new Exception("No scene triggers found in level"); 
			}

			// 2. Find the trigger where the ID matches AND it belongs to this level's logic
			var targetTrigger = sceneTriggers
				.Cast<SceneTrigger>()
				.FirstOrDefault(st => st.CurrentLevelTrigger == triggerId);

			if (targetTrigger == null)
			{
				Logger.Error($"Could not find Trigger ID {triggerId} in {CurrentLevel.LevelName}");
				return;
			}

			// 3. Move the player
			Logger.Info($"Warping to: {targetTrigger.Name} (ID: {targetTrigger.CurrentLevelTrigger})");
			
			Vector2 spawnPos = targetTrigger.GlobalPosition + (targetTrigger.EntryDirection * Globals.GridSize);
			GameManager.GetPlayer().GlobalPosition = spawnPos;

// Move the follower to be behind the player
if (followerInstance != null && IsInstanceValid(followerInstance))
{
Vector2 followerPos = spawnPos - (targetTrigger.EntryDirection * Globals.GridSize);
followerInstance.GlobalPosition = followerPos;
}
}

private void SpawnFollower(Player player, Vector2 playerPosition)
{
// Remove old follower if it exists
if (followerInstance != null && IsInstanceValid(followerInstance))
{
followerInstance.QueueFree();
}

// Create new follower
var followerScene = GD.Load<PackedScene>("res://scenes/charecters/follower.tscn");
followerInstance = followerScene.Instantiate<Follower>();

// Add to the same parent as the player (GameViewPort)
GameManager.GetGameViewPort().AddChild(followerInstance);

// Position follower one tile behind the player (down direction by default)
followerInstance.GlobalPosition = playerPosition + new Vector2(0, Globals.GridSize);

// Set up the follower's input references
var followerInput = followerInstance.GetNode<FollowerInput>("Input");
if (followerInput != null)
{
followerInput.PlayerNode = player;
followerInput.FollowerNode = followerInstance;
}
}

public static Follower GetFollower()
{
return followerInstance;
}
 
 
		public async Task FadeOut()
		{
			if (FadeRect == null) { Game.Core.Logger.Error("FadeRect is null in FadeOut"); return; }
			Tween tween = CreateTween();
			tween.TweenProperty(FadeRect, "color:a", 1.0, 0.25);
			await ToSignal(tween, "finished");
		}
		public async Task FadeIn()
		{
			 if (FadeRect == null) { Game.Core.Logger.Error("FadeRect is null in FadeIn"); return; }
			Tween tween = CreateTween();
			tween.TweenProperty(FadeRect, "color:a", 0.0, 0.25);
			await ToSignal(tween, "finished");
		}

		public void ResetSession()
		{
			if (IsInstanceValid(CurrentLevel))
			{
				CurrentLevel.GetParent()?.RemoveChild(CurrentLevel);
				CurrentLevel.QueueFree();
			}
			CurrentLevel = null;
			
			if (AllLevels != null)
			{
				foreach (var level in AllLevels)
				{
					if (IsInstanceValid(level))
					{
						level.QueueFree();
					}
				}
				AllLevels.Clear();
}

// Clean up follower instance
if (followerInstance != null && IsInstanceValid(followerInstance))
{
followerInstance.QueueFree();
followerInstance = null;
}

isChanging = false;
		}

		public static Level GetCurrentLevel()
		{
			return Instance.CurrentLevel;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event.IsActionPressed("close"))
			{
				LeaveGame();
			}
		}

		public static void LeaveGame()
		{
			if (SaveManager.Instance != null)
			{
				SaveManager.Instance.QuitGame();
			}
		}
	}
}
