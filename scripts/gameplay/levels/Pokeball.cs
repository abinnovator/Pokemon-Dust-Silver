using Godot;
using Game.Core;
using System.Threading.Tasks;

namespace Game.Gameplay;

public partial class Pokeball : StaticBody2D
{
	[ExportCategory("Pokemon Info")]
	[Export] public PokemonStarter PokemonName;
	[Export] public PokemonType Type;
	[Export] public Texture2D PokemonSprite; 

	[ExportCategory("Visuals")]
	[Export] public Sprite2D BallSprite;
	
	[Export] public GpuParticles2D SuccessParticles;
	[Export] public AudioStreamPlayer AudioPlayer;

	// 1. The Safety Lock: This prevents the interaction from repeating
	private bool _isProcessing = false;

	private readonly System.Collections.Generic.Dictionary<PokeballType, AtlasTexture> _textures = new ()
	{
		{ PokeballType.Closed, GD.Load<AtlasTexture>("res://resources/textures/pokeball_normal_closed.tres") },
		{ PokeballType.Open, GD.Load<AtlasTexture>("res://resources/textures/pokeball_normal_open.tres") },
	};

	public override void _Ready()
	{
		BallSprite ??= GetNodeOrNull<Sprite2D>("Sprite2D");
	}

	public async Task OnInteractAsync()
	{
		// 2. Immediate Check: If we are already in the middle of this logic, STOP
		if (_isProcessing) return;
		
		_isProcessing = true; // Lock the door

		Game.Core.Logger.Info($"Interacted with {PokemonName} Pokéball");
		
		// 3. Visual/Audio Juice
		BallSprite.Texture = _textures[PokeballType.Open];
		AudioPlayer?.Play();
		if (SuccessParticles != null) SuccessParticles.Emitting = true;
		if (SaveManager.Instance.CurrentSave.StoryProgress != PlayerStoryState.HAS_STARTER){
			bool wantsToTake = await MessageManager.PlayText(
				$"It's a {PokemonName}!", 
				$"This {Type} type Pokemon looks energetic.",
				$"Do you want to choose {PokemonName}?"
			);
			if (wantsToTake)
			{
				if (SaveManager.Instance.CurrentSave != null)
				{
					// Set story progress and map the starter enum
					SaveManager.Instance.CurrentSave.StoryProgress = PlayerStoryState.HAS_STARTER;
					SaveManager.Instance.CurrentSave.ChosenStarter = PokemonName switch
					{
						PokemonStarter.BULBASAUR => StarterChoice.BULBASAUR,
						PokemonStarter.CHARMANDER => StarterChoice.CHARMANDER,
						PokemonStarter.SQUIRTLE => StarterChoice.SQUIRTLE,
						_ => StarterChoice.NONE 
					};
					
					// Finalize the save to disk
					SaveManager.Instance.SaveToDisk();
					Game.Core.Logger.Info($"{PokemonName} has been saved to your trainer file.");
				}
				
				// Show success message and wait for it to close
				await MessageManager.PlayText($"You received {PokemonName}!");
				BallSprite.Texture = _textures[PokeballType.Closed];

			}
			else
			{
				// Reset the ball if the player cancels
				Game.Core.Logger.Info($"Player declined {PokemonName}.");
				BallSprite.Texture = _textures[PokeballType.Closed];
			}

			// 6. Cleanup: Wait a fraction of a second so the input isn't detected again
			await Task.Delay(200); 
			_isProcessing = false; // Unlock the door
		}else {
			await MessageManager.PlayText("You already have a starter Pokemon!");
			_isProcessing = false; // Unlock the door
		}
    }
}
