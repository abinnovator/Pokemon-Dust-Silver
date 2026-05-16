using Game.Core;
using Godot;
using System;

namespace Game.Gameplay{
	public partial class SceneTrigger : Area2D
	{

		[ExportCategory("Target Scene Variables")]
		[Export]
		public LevelName TargetLevelName;
		[Export]
		public int TargetLevelTrigger = 0;
		[ExportCategory("Current Scene Vars")]
		[Export]
		public int CurrentLevelTrigger = 0;
		[Export]
		public Vector2 EntryDirection;
		[Export]
		public bool Locked = false;
		[Export]
		public bool MapPoint = false;
		[Export] public PlayerStoryState RequiredStoryState;
		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;

		}
		public void OnBodyEntered(Node2D body)
		{
			if (body is not Player)
			{
				return;
			}
			if (SceneManager.isChanging)
			{
				return;
			}
			Game.Core.Logger.Info($"Scene Trigger Entered by: {body.Name} (Type: {body.GetType().Name})");
			if (Locked)
			{
				if (SaveManager.Instance.CurrentSave.CompletedStoryProgress.Contains(RequiredStoryState))
				{
					Game.Core.Logger.Info("Scene Trigger is Unlocked");
					Locked = false;
				}
				else
				{
					Game.Core.Logger.Info("Scene Trigger is Locked");
					return;
				}
			}
			if (MapPoint)
			{
				Game.Core.Logger.Info("Scene Trigger is Map Point");
				return;
			}
			Game.Core.Logger.Info($"Changing Level to: {TargetLevelName}, Trigger: {TargetLevelTrigger}");
			SceneManager.ChangeLevel(levelName: TargetLevelName, trigger: TargetLevelTrigger);
		}
		
		public override void _EnterTree()
		{
			AddToGroup(LevelGroups.SCENETRIGGERS.ToString());
		}
		public override void _ExitTree()
		{
			RemoveFromGroup(LevelGroups.SCENETRIGGERS.ToString());
		}

	}
}
