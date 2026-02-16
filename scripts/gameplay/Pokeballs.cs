using Game.Core;
using Godot;
using System;
using Logger = Game.Core.Logger;

namespace Game.Gameplay; // Added namespace to keep it consistent with your other files

public partial class Pokeballs : Node2D
{
	public override void _Ready()
	{
		// 1. Safety Check: Make sure the save data actually exists before checking it
		if (SaveManager.Instance?.CurrentSave == null)
		{
			return;
		}

		// 2. Check the enum state
		if (SaveManager.Instance.CurrentSave.StoryProgress == PlayerStoryState.HAS_STARTER)
		{
			// Hide the entire container of Pokéballs
			this.Visible = false; 
			Logger.Info("Pokeballs: Starter was already chosen, hiding balls.");
			
			// 3. Optional: Disable processing so they don't take up CPU power
			ProcessMode = ProcessModeEnum.Disabled;
		}
	}
}
