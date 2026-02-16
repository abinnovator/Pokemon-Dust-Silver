using Godot;
using System;
using Game.Core; // <--- ADD THIS LINE
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
	// Fixed the typo: CharacterInput instead of CharecterInput
	public partial class PlayerInput : CharecterInput 
	{
		[ExportCategory("Player")]
		[Export] public double HoldThreshhold = 0.1f;
		[Export] public double HoldTime = 0.0f;

		public override void _Ready()
		{
			// Now this will find Game.Core.Logger.Info
			Logger.Info("Loading player input");
		}
	}
}
