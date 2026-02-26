using Godot;
using System;

namespace Game.Core
{

	public partial class GameManager : Node
	{
		public static GameManager Instance {get;private set;}

		[ExportCategory("Nodes")]
		[Export]

		public SubViewport GameViewPort ;
		[ExportCategory("Vars")]
		[Export]
		public Player Player;


		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Instance = this;
			Logger.Info("GameManager ready");
			
			// Load Start Screen instead of loading level directly
			var startScreen = GD.Load<PackedScene>("res://scenes/ui/StartScreen.tscn").Instantiate();
			AddChild(startScreen);

			// Instantiate Pokemon Selection UI
			var pokemonSelectionUi = GD.Load<PackedScene>("res://scenes/core/pokemon_selection_ui.tscn").Instantiate();
			AddChild(pokemonSelectionUi);
		}

		public static SubViewport GetGameViewPort()
		{
			return Instance.GameViewPort;
		}
		public static Player AddPlayer(Player player)
		{
			Instance.GameViewPort.AddChild(player);
			Instance.Player = player;
			return Instance.Player;
		}
		public static bool IsPlayerMovementLocked { get; set; } = false;

		public static Player GetPlayer()
		{
			return Instance.Player;
		}
	}
	

}
