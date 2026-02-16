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
		// Called when the node enters the scene tree for the first time.
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
				Game.Core.Logger.Info("Scene Trigger is Locked");
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
