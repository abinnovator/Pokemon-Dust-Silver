using Game.Core;
using Godot;
using System;

public partial class PlayerPokemon : Sprite2D
{
	public BattleManager battleManager;
	private const string spriteFolderPath = "res://assets/pokemon/";
	private Sprite2D _playerPokemon;
	private Sprite2D _oppPokemon;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	

	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
