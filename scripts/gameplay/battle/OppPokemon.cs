using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;

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
                Logger.Info($"Loaded opponent pokemon sprite: {path}");
            }
            else
            {
                Logger.Warning($"Could not find opponent pokemon sprite at: {path}");
            }
        }
    }
}
