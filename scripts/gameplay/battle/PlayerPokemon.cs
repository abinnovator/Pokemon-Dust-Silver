using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
    public partial class PlayerPokemon : Sprite2D
    {
        private const string spriteFolderPath = "res://assets/pokemon/";

        public void Setup(PokemonID id)
        {
            string pokemonName = id.ToString().ToLower();
            string path = $"{spriteFolderPath}{pokemonName}_back.png";
            
            try 
            {
                var tex = GD.Load<Texture2D>(path);
                if (tex != null)
                {
                    Texture = tex;
                    Logger.Info($"Successfully loaded: {path}");
                }
                else 
                {
                    Logger.Warning($"GD.Load returned null for: {path}");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Critical error loading sprite {path}: {e.Message}");
            }
        }
    }
}
