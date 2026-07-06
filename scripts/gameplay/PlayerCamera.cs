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
			CallDeferred(Camera2D.MethodName.ResetSmoothing);
		}

		public override void _Process(double delta)
		{
			if (CurrentLevel != SceneManager.Instance.CurrentLevel)
			{
				CurrentLevel = SceneManager.Instance.CurrentLevel;
				CallDeferred(nameof(DeferredUpdate));
			}
		}

		private void DeferredUpdate()
		{
			UpdateCameraLimits();
			CallDeferred(Camera2D.MethodName.ResetSmoothing);
			CallDeferred(nameof(SecondReset));
		}

		private void SecondReset()
		{
			CallDeferred(Camera2D.MethodName.ResetSmoothing);
}

		public void UpdateCameraLimits()
		{
			LimitTop = CurrentLevel.top;
			LimitBottom = CurrentLevel.bottom;
			LimitLeft = CurrentLevel.left;
			LimitRight = CurrentLevel.right;

		}
	}
}	
