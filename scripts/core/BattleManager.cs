using Game.Core;
using Godot;
using System;
using Logger = Game.Core.Logger;

public partial class BattleManager : CanvasLayer
{
	[ExportCategory("Pokemon Stats")]
	[Export] public PokemonID playerPokemonId;
	[Export] public PokemonID opponentPokemonId;

	private const string spriteFolderPath = "res://assets/pokemon/";
	
	private Sprite2D _playerSprite;
	private Sprite2D _oppSprite;
	private RichTextLabel _playerName;
	private RichTextLabel _oppName;
	private TextureButton _runButton; // Changed from Button to TextureButton

	public override void _Ready()
	{
		_playerSprite = GetNode<Sprite2D>("Ui/PlayerPokemon");
		_oppSprite = GetNode<Sprite2D>("Ui/OppPokemon");

		_playerName = GetNode<RichTextLabel>("Ui/Control/PokemonName");
		_oppName = GetNode<RichTextLabel>("Ui/Control/OppPokemonName");

		// UPDATE THIS PATH: Change 'Control' to 'Controls' if that's where the button is
		// Change <Button> to <TextureButton>
		_runButton = GetNode<TextureButton>("Ui/Controls/Run"); 
		_runButton.Pressed += OnRunButtonPressed;

		SetPlayerPokemon(playerPokemonId);
		SetOppPokemon(opponentPokemonId);

		var playerPokemon = PokeBase.LoadPokemon(playerPokemonId);
		var oppPokemon = PokeBase.LoadPokemon(opponentPokemonId);

		if (playerPokemon == null || oppPokemon == null)
		{
			Logger.Error("Failed to load pokemon");
			QueueFree();
			return;
		}
		else {
			Logger.Info($"Loaded pokemon {playerPokemon} and {oppPokemon}");
		}
	}

	public void SetPlayerPokemon(PokemonID pokemonId)
	{
		string path = $"{spriteFolderPath}{pokemonId.ToString().ToLower()}_back.png";
		_playerSprite.Texture = GD.Load<Texture2D>(path);
		_playerName.Text = pokemonId.ToString().ToUpper(); //
	}

	public void SetOppPokemon(PokemonID pokemonId)
	{
		string path = $"{spriteFolderPath}{pokemonId.ToString().ToLower()}_front.png";
		_oppSprite.Texture = GD.Load<Texture2D>(path);
		_oppName.Text = pokemonId.ToString().ToUpper(); //
	}

	public void OnRunButtonPressed()
	{
		GD.Print("Got away safely!");
		QueueFree(); // Removes the battle scene
	}
}
