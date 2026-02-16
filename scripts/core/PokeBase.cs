using Game.Core;
using Game.Gameplay;
using Godot;
using Logger = Game.Core.Logger;


public partial class PokeBase : Node
{
    private const string pokemonPath = "res://resources/pokemon/";
	public static PokemonResource LoadPokemon(PokemonID id)
	{
		var pokemon = GD.Load<PokemonResource>(pokemonPath + id.ToString() + ".tres");
		if (pokemon == null)
		{
			Logger.Error($"Pokemon {id} not found");
			return null;
		}
        return pokemon;
		
	}
}