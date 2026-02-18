using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;

namespace Game.Gameplay
{
    /// <summary>
    /// The root controller for the Battle Scene.
    /// Owns the StateMachine and references to UI components.
    /// </summary>
    public partial class BattleMain : CanvasLayer
    {
        [ExportCategory("UI References")]
        [Export] public RichTextLabel PlayerNameLabel;
        [Export] public RichTextLabel OpponentNameLabel;
        [Export] public TextureProgressBar PlayerHPBar;
        [Export] public TextureProgressBar EnemyHPBar;
        [Export] public Control CommandMenu;
        [Export] public Control MoveMenu;

        [ExportCategory("Buttons")]
        [Export] public BaseButton BattleButton;
        [Export] public BaseButton RunButton;
        [Export] public BaseButton BackButton;
        [Export] public BaseButton[] MoveButtons;

        [ExportCategory("Systems")]
        [Export] public BattleStateMachine StateMachine;
        [Export] public PlayerPokemon PlayerSprite;
        [Export] public OppPokemon OpponentSprite;

        public PokemonID PlayerID { get; private set; }
        public PokemonID OpponentID { get; private set; }

        public override void _Ready()
        {
            Logger.Info("BattleMain initializing...");
            
            // Get data from Autoload
            if (BattleManager.Instance != null)
            {
                PlayerID = BattleManager.Instance.playerPokemonId;
                OpponentID = BattleManager.Instance.opponentPokemonId;
            }

            // Emergency Fallback: If player ID is none, fetch it again or default to bulbasaur
            if (PlayerID == PokemonID.none)
            {
                Logger.Warning("PlayerID was 'none' at runtime. Recovering...");
                PlayerID = BattleManager.Instance?.GetSavedPlayerPokemon() ?? PokemonID.bulbasaur;
            }

            // Basic UI Setup
            if (PlayerNameLabel != null) PlayerNameLabel.Text = PlayerID != PokemonID.none ? PlayerID.ToString() : "Player";
            if (OpponentNameLabel != null) OpponentNameLabel.Text = OpponentID != PokemonID.none ? OpponentID.ToString() : "Opponent";

            if (PlayerSprite != null) PlayerSprite.Setup(PlayerID);
            if (OpponentSprite != null) OpponentSprite.Setup(OpponentID);

            // Connect Signals
            if (BattleButton != null) BattleButton.Pressed += () => OnBattleButtonPressed();
            if (RunButton != null) RunButton.Pressed += () => EndBattle();
            if (BackButton != null) BackButton.Pressed += () => OnBackButtonPressed();

            if (MoveButtons != null)
            {
                foreach (var btn in MoveButtons)
                {
                    if (btn != null)
                    {
                        // Use safe casting to access .Text if it's a regular Button
                        // If it's a TextureButton, this will be null and default to "Move"
                        string moveName = (btn as Button)?.Text ?? btn.Name.ToString();
                        
                        btn.Pressed += () => OnMoveSelected(moveName);
                    }
                }
            }

            // Start the state machine
            if (StateMachine != null)
            {
                StateMachine.StartBattle();
            }
        }

        public void UpdateLog(string message)
        {
            Logger.Info($"BATTLE LOG: {message}");
        }

        public void EndBattle()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.EndBattle();
        }

        private void OnBattleButtonPressed()
        {
            if (CommandMenu != null) CommandMenu.Visible = false;
            if (MoveMenu != null) MoveMenu.Visible = true;
        }

        private void OnBackButtonPressed()
        {
            if (CommandMenu != null) CommandMenu.Visible = true;
            if (MoveMenu != null) MoveMenu.Visible = false;
        }

        private void OnMoveSelected(string moveName)
        {
            UpdateLog($"Player used {moveName}!");
            
            if (StateMachine != null)
            {
                StateMachine.ChangeState("EnemyTurnState");
            }
        }
    }
}
