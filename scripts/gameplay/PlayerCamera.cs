using Game.Core;
using Godot;
using System;

namespace Game.Gameplay
{
	public partial class PlayerCamera : Camera2D
	{
		[ExportCategory("Camera Vars")]
		[Export]
		public Level CurrentLevel;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			CurrentLevel = SceneManager.Instance.CurrentLevel;
			UpdateCameraLimits();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (CurrentLevel != SceneManager.Instance.CurrentLevel)
			{
				CurrentLevel = SceneManager.Instance.CurrentLevel;
				UpdateCameraLimits();
			}
		}
		public void UpdateCameraLimits()
		{
			// Level class uses lowercase fields for limits
			LimitTop = CurrentLevel.top;
			LimitBottom = CurrentLevel.bottom;
			LimitLeft = CurrentLevel.left;
			LimitRight = CurrentLevel.right;

		}
	}
}	
