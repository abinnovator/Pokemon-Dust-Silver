using Game.Core;
using Godot;
using System;

namespace Game.Gameplay
{
    public partial class PlayerPokemon : Sprite2D
    {
        private const string spriteFolderPath = "res://assets/pokemon/";

        public void Setup(PokemonID id)
        {
            string pokemonName = id.ToString().ToLower();
            string path = $"{spriteFolderPath}{pokemonName}_back.png";
            
            if (FileAccess.FileExists(path))
            {
                Texture = GD.Load<Texture2D>(path);
                Game.Core.Logger.Info($"Loaded player pokemon sprite: {path}");
            }
            else
            {
                Game.Core.Logger.Warning($"Could not find player pokemon sprite at: {path}");
            }
        }
    }
}
