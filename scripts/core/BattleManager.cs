using Godot;
using System;
using System.Threading.Tasks;
using Game.Gameplay;

namespace Game.Core
{
    /// <summary>
    /// Singleton Autoload responsible for scene transitions and overworld state saving.
    /// It does NOT handle battle logic; that is handled by BattleMain and the BattleStateMachine.
    /// </summary>
    public partial class BattleManager : Node
    {
        public static BattleManager Instance { get; private set; }

        // --- Overworld State Savings ---
        public LevelName SavedLevelName;
        public Vector2 SavedPlayerPosition;
        public StoryNpcInputConfig CurrentBattleConfig;

        // --- Battle Config (Passed to the local battle scene) ---
        public PokemonID opponentPokemonId;
        public PokemonID playerPokemonId;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Logger.Info("BattleManager (Autoload) initialized");
            }
            else
            {
                QueueFree();
            }
        }

        public void StartBattle(StoryNpcInputConfig config)
        {
            if (SceneManager.isChanging) return;

            Logger.Info("StartBattle (Trainer) called");
            SaveOverworldState();
            CurrentBattleConfig = config;

            // Prepare IDs for trainer battle
            if (Enum.TryParse(config.PokemonID.ToString(), true, out PokemonID id))
            {
                opponentPokemonId = id;
            }
            playerPokemonId = GetSavedPlayerPokemon();

            SceneManager.ChangeLevel(LevelName.battle_scene, 0, false);
        }

        public void StartWildBattle(PokemonID wildId, PokemonID playerId)
        {
            if (SceneManager.isChanging) return;

            Logger.Info($"StartWildBattle called for {wildId}");
            SaveOverworldState();
            
            opponentPokemonId = wildId;
            playerPokemonId = playerId;

            // Instantiate battle UI directly as a overlay/overlay scene
            var battleScene = GD.Load<PackedScene>("res://scenes/core/battle_ui.tscn").Instantiate();
            GetTree().Root.AddChild(battleScene);
        }

        public void EndBattle()
        {
            Logger.Info("EndBattle called, returning to overworld");
            ReturnToOverworld();
        }

        private void SaveOverworldState()
        {
            SavedLevelName = SceneManager.GetCurrentLevel().LevelName;
            SavedPlayerPosition = GameManager.GetPlayer().GlobalPosition;
        }

        private async void ReturnToOverworld()
        {
            await SceneManager.Instance.FadeOut();
            await SceneManager.Instance.GetLevel(SavedLevelName);
            
            var player = GameManager.GetPlayer();
            if (player != null)
            {
                player.GlobalPosition = SavedPlayerPosition;
            }

            await SceneManager.Instance.FadeIn();
            Logger.Info($"Returned to {SavedLevelName} at {SavedPlayerPosition}");
        }

        private PokemonID GetSavedPlayerPokemon()
        {
            try
            {
                var save = ResourceLoader.Load<PlayerSaveResource>("user://savegame.tres");
                if (save != null)
                {
                    var id = MapStarterToPokemonID(save.ChosenStarter);
                    if (id != PokemonID.none) return id;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error loading player pokemon from save: {e.Message}");
            }
            return PokemonID.bulbasaur;
        }

        private PokemonID MapStarterToPokemonID(StarterChoice choice)
        {
            return choice switch
            {
                StarterChoice.BULBASAUR => PokemonID.bulbasaur,
                StarterChoice.CHARMANDER => PokemonID.charmander,
                StarterChoice.SQUIRTLE => PokemonID.squirtle,
                _ => PokemonID.none
            };
        }
    }
}
