using Game.Core;
using Godot;
using System;

namespace Game.Gameplay
{
    public partial class OppPokemon : Sprite2D
    {
        private const string spriteFolderPath = "res://assets/pokemon/";

        public void Setup(PokemonID id)
        {
            string pokemonName = id.ToString().ToLower();
            string path = $"{spriteFolderPath}{pokemonName}_front.png";
            
            if (FileAccess.FileExists(path))
            {
                Texture = GD.Load<Texture2D>(path);
                Game.Core.Logger.Info($"Loaded opponent pokemon sprite: {path}");
            }
            else
            {
                Game.Core.Logger.Warning($"Could not find opponent pokemon sprite at: {path}");
            }
        }
    }
}
