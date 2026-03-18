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
        public GymNpcInputConfig CurrentGymConfig { get; private set; }

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

        public void StartGymBattle(GymNpcInputConfig config)
        {
            if (SceneManager.isChanging) return;

            Logger.Info($"StartGymBattle called for {config.LeaderName}");
            SaveOverworldState();
            CurrentGymConfig = config;

            // Use the first Pokémon in the team as the initial opponent ID
            opponentPokemonId = PokemonID.none;
            foreach (var entry in config.TrainerTeam)
            {
                opponentPokemonId = entry.Key;
                break;
            }
            playerPokemonId = GetSavedPlayerPokemon();

            var battleScene = GD.Load<PackedScene>("res://scenes/core/trainer_battle_ui.tscn").Instantiate();
            GetTree().Root.AddChild(battleScene);
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

            // Use the same trainer battle UI as gym battles
            var battleScene = GD.Load<PackedScene>("res://scenes/core/trainer_battle_ui.tscn").Instantiate();
            GetTree().Root.AddChild(battleScene);
        }

        public void StartWildBattle(PokemonID wildId, PokemonID playerId)
        {
            if (SceneManager.isChanging) return;

            Logger.Info($"StartWildBattle called for {wildId}");
            SaveOverworldState();
            
            opponentPokemonId = wildId;
            
            // If playerId is none, try to fetch it from save
            if (playerId == PokemonID.none)
            {
                playerPokemonId = GetSavedPlayerPokemon();
            }
            else
            {
                playerPokemonId = playerId;
            }

            // Instantiate battle UI directly as a overlay/overlay scene
            var battleScene = GD.Load<PackedScene>("res://scenes/core/battle_ui.tscn").Instantiate();
            GetTree().Root.AddChild(battleScene);
        }

        public void EndBattle()
        {
            Logger.Info("EndBattle called, returning to overworld");
            CurrentGymConfig = null;
            ReturnToOverworld();
        }

        private void SaveOverworldState()
        {
            SavedLevelName = SceneManager.GetCurrentLevel().LevelName;
            SavedPlayerPosition = GameManager.GetPlayer().GlobalPosition;
        }

        private async void ReturnToOverworld()
        {
            if (SceneManager.Instance != null)
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
        }

        public PokemonID GetSavedPlayerPokemon()
        {
            // 1. Try SaveManager.Instance (In-memory current session)
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                var id = MapStarterToPokemonID(SaveManager.Instance.CurrentSave.ChosenStarter);
                if (id != PokemonID.none) return id;
            }

            // 2. Try loading from disk directly as fallback
            try
            {
                if (FileAccess.FileExists("user://savegame.tres"))
                {
                    var save = ResourceLoader.Load<PlayerSaveResource>("user://savegame.tres");
                    if (save != null)
                    {
                        var id = MapStarterToPokemonID(save.ChosenStarter);
                        if (id != PokemonID.none) return id;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error loading player pokemon from save: {e.Message}");
            }

            // 3. Absolute fallback: If nothing is found, we MUST have a visible pokemon
            Logger.Warning("No player pokemon found in save. Falling back to Bulbasaur.");
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
