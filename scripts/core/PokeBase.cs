using System.Runtime.CompilerServices;
using Game.Core;
using Game.Gameplay;
using Godot;
using Logger = Game.Core.Logger;


public partial class PokeBase : Node
{
    private const string pokemonPath = "res://resources/pokemon/";
	private const string movePath = "res://resources/Moves/";
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
	public static MoveResource LoadMove (MoveID id){
		var move = GD.Load<MoveResource>(movePath + id.ToString() + ".tres");
		if (move == null)
		{
			Logger.Error($"Move {id} not found");
			return null;
		}
        return move;
	}

    public static MoveResource LoadMove(string id)
    {
        var move = GD.Load<MoveResource>(movePath + id + ".tres");
        if (move == null)
        {
            Logger.Error($"Move {id} not found");
            return null;
        }
        return move;
    }
}
