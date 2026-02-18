using Godot;
using System;
using Game.Core;

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

        [ExportCategory("Components")]
        [Export] public BattleStateMachine StateMachine;
        [Export] public PlayerPokemon PlayerSprite;
        [Export] public OppPokemon OpponentSprite;

        public PokemonID PlayerID { get; private set; }
        public PokemonID OpponentID { get; private set; }

        public override void _Ready()
        {
            Game.Core.Logger.Info("BattleMain initializing...");
            
            // Get data from Autoload
            PlayerID = BattleManager.Instance.playerPokemonId;
            OpponentID = BattleManager.Instance.opponentPokemonId;

            // Basic UI Setup
            if (PlayerNameLabel != null) PlayerNameLabel.Text = PlayerID.ToString();
            if (OpponentNameLabel != null) OpponentNameLabel.Text = OpponentID.ToString();

            if (PlayerSprite != null) PlayerSprite.Setup(PlayerID);
            if (OpponentSprite != null) OpponentSprite.Setup(OpponentID);

            // Start the state machine
            if (StateMachine != null)
            {
                StateMachine.StartBattle();
            }
        }

        public void UpdateLog(string message)
        {
            Game.Core.Logger.Info($"[Battle] {message}");
            // TODO: Implement on-screen log label if available
        }

        public void EndBattle()
        {
            BattleManager.Instance.EndBattle();
            
            // If this was added directly to root (wild battle overlay)
            if (GetParent() == GetTree().Root)
            {
                QueueFree();
            }
        }
    }
}
