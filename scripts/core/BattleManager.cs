using Godot;
using System;
using System.Threading.Tasks;
using Game.Gameplay;

namespace Game.Core
{
    public partial class BattleManager : Node
    {
        public static BattleManager Instance { get; private set; }

        public LevelName SavedLevelName;
        public Vector2 SavedPlayerPosition;
        public StoryNpcInputConfig CurrentBattleConfig;

        public PokemonID opponentPokemonId;
        public PokemonID playerPokemonId;
        public int opponentPokemonLevel;
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

            if (Enum.TryParse(config.PokemonID.ToString(), true, out PokemonID id))
            {
                opponentPokemonId = id;
            }
            playerPokemonId = GetSavedPlayerPokemon();

            var battleScene = GD.Load<PackedScene>("res://scenes/core/trainer_battle_ui.tscn").Instantiate();
            GetTree().Root.AddChild(battleScene);
        }

        public void StartWildBattle(PokemonID wildId, PokemonID playerId, int minLevel = 2, int maxLevel = 10)
        {
            if (SceneManager.isChanging) return;

            opponentPokemonLevel = GD.RandRange(minLevel, maxLevel);
            Logger.Info($"StartWildBattle called for {wildId} at Lv.{opponentPokemonLevel}");
            SaveOverworldState();

            opponentPokemonId = wildId;

            if (playerId == PokemonID.none)
            {
                playerPokemonId = GetSavedPlayerPokemon();
            }
            else
            {
                playerPokemonId = playerId;
            }

            var battleScene = GD.Load<PackedScene>("res://scenes/core/battle_ui.tscn").Instantiate();
            GetTree().Root.AddChild(battleScene);
        }

        public void EndBattle()
        {
            Logger.Info("EndBattle called, returning to overworld");
            CurrentGymConfig = null;
            CurrentBattleConfig = null;
            opponentPokemonId = PokemonID.none;
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
                
                GameManager.IsPlayerMovementLocked = false;
                
                Logger.Info($"Returned to {SavedLevelName} at {SavedPlayerPosition}");
            }
        }

        public PokemonID GetSavedPlayerPokemon()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                var id = MapStarterToPokemonID(SaveManager.Instance.CurrentSave.ChosenStarter);
                if (id != PokemonID.none) return id;
            }

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
