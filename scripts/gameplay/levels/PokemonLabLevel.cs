using Game.Core;
using Godot;
using System.Threading.Tasks;

namespace Game.Gameplay
{
	
	public partial class PokemonLabLevel : Level
	{
		[ExportCategory("Oak")]
		[Export] public OakNpc OakNode;
		private bool _hasTriggeredOakDialogue = false;

		public override void _Ready()
		{
			base._Ready();
			
			// Trigger Oak's dialogue when entering if story progress is appropriate
			CallDeferred(nameof(CheckAndTriggerOak));
		}

		private async void CheckAndTriggerOak()
		{
			// Wait a frame for scene to fully load
			await ToSignal(GetTree(), "process_frame");
			
			if (SaveManager.Instance?.CurrentSave == null) return;
			
			// Only trigger if player hasn't chosen a starter yet and hasn't seen this dialogue
			if (SaveManager.Instance.CurrentSave.StoryProgress == PlayerStoryState.NEW_GAME && !_hasTriggeredOakDialogue)
			{
				_hasTriggeredOakDialogue = true;
				
				// Find Oak NPC in the scene

				if (OakNode != null)
				{
					// Wait a moment for player to settle
					await Task.Delay(500);
					OakNode._stateMachine.ChangeState("Roam");
					await OakNode.MoveToPosition(new Vector2(88, 136));
					await OakNode.FaceDirection(Vector2.Up);

					// Trigger Oak's dialogue (he faces down towards the entrance)
					await OakNode.PlayMessage(Vector2.Up);
					
					// Update story state so this doesn't trigger again
					SaveManager.Instance.CurrentSave.StoryProgress = PlayerStoryState.MET_OAK;
					SaveManager.Instance.SaveToDisk();
				}else{
					GD.PrintErr("OakNode is null");
				}
			}
		}
	}
}
