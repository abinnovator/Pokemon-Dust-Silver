using Game.Core;
using Game.Utilities;
using Godot;
using System.Linq;

namespace Game.Gameplay;

public partial class StoryNpcInterceptState : State
{
	[ExportCategory("State Vars")]
	[Export] public StoryNpcInput NpcInput;
	[Export] public CharacterMovement CharacterMovement;

	public override void EnterState()
	{
		base.EnterState();
		RunIntercept();
	}

	private async void RunIntercept()
	{
		var storyNpc = (StoryNpc)StateOwner;
		var config = NpcInput.NpcInputConfig;
		if (config == null) return;

		// Lock player movement
		GameManager.IsPlayerMovementLocked = true;

		// Walk to player tile-by-tile
		await storyNpc.WalkToPlayer();

		// Face the player
		var player = GameManager.GetPlayer();
		if (player != null)
		{
			storyNpc.FaceToward(player.Position);
		}

		// Play forced dialogue
		if (config.InterceptMessages.Count > 0)
		{
			await MessageManager.PlayText(null, config.InterceptMessages.ToArray());
		}

		// Unlock player movement
		GameManager.IsPlayerMovementLocked = false;

		// Return to Roam
		StateMachine.ChangeState("Roam");
	}
}
