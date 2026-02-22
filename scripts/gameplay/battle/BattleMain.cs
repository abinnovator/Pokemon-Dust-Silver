using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;
using System.Threading.Tasks;

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
        [Export] public Control PartyMenu;


        [ExportCategory("Buttons")]
        [Export] public BaseButton BattleButton;
        [Export] public BaseButton RunButton;
        [Export] public BaseButton BackButton;
        [Export] public BaseButton PokemonBackButton;
        [Export] public BaseButton PokemonMenuButton;
        [Export] public BaseButton[] MoveButtons;
        [Export] public BaseButton[] PokemonButtons;

        [ExportCategory("Systems")]
        [Export] public BattleStateMachine StateMachine;
        [Export] public PlayerPokemon PlayerSprite;
        [Export] public OppPokemon OpponentSprite;

        public PokemonID PlayerID { get; private set; }
        public PokemonID OpponentID { get; private set; }
        private PokemonResource _playerPokemon;
        private PokemonResource _oppPokemon;
        private int _totalPokemonHp;
        private int _oppPokemonHp;
        private int _oppPokemonLevel;
        private int _playerPokemonLevel;
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

            Logger.Info($"BattleMain _Ready. PartyMenu: {PartyMenu?.Name ?? "NULL"}, PokemonButtons Count: {PokemonButtons?.Length ?? 0}");
            
            // Connect Signals
            if (BattleButton != null) BattleButton.Pressed += () => OnBattleButtonPressed();
            if (RunButton != null) RunButton.Pressed += () => RunAway();
            if (BackButton != null) BackButton.Pressed += () => OnBackButtonPressed();
            if (PokemonMenuButton != null) PokemonMenuButton.Pressed += () => OnPokemonMenuButtonPressed();
            if (PokemonBackButton != null) PokemonBackButton.Pressed += () => OnPokemonBackButtonPressed();
            
            if (PokemonButtons!=null){
                Logger.Info($"Connecting {PokemonButtons.Length} Party Slot Buttons");
                foreach (var button in PokemonButtons){
                    if (button != null)
                        button.Pressed += () => {
                            // Logic for selecting a pokemon in the menu could go here
                            Logger.Info($"Party slot {button.Name} pressed");
                        };
                    else
                        Logger.Warning("Found NULL button in PokemonButtons array");
                }
            }
            _playerPokemon = PokeBase.LoadPokemon(PlayerID);
            _oppPokemon = PokeBase.LoadPokemon(OpponentID);
            _totalPokemonHp = _playerPokemon.BaseHp;
            _oppPokemonHp = _oppPokemon.BaseHp;
            PlayerHPBar.MaxValue = _totalPokemonHp;
            EnemyHPBar.MaxValue = _oppPokemonHp;

            Logger.Info($"Player Pokemon Learnable Moves: {string.Join(", ", _playerPokemon.LearnableMoves)}");

            if (MoveButtons != null)
            {
                foreach (var btn in MoveButtons)
                {
                    if (btn != null)
                    {
                        btn.Pressed += () => 
                        {
                            string moveName = (btn as Button)?.Text ?? btn.Name.ToString();
                            OnMoveSelectedAsync(moveName);
                        };
                    }
                }

            }

            // Start the state machine
            if (StateMachine != null)
            {
                StateMachine.StartBattle();
            }

            // Safety check: Ensure PartyDetails has the player's current pokemon if it's empty
            if (SaveManager.Instance?.CurrentSave != null)
            {
                Logger.Info($"Current PartyDetails count: {SaveManager.Instance.CurrentSave.PartyDetails.Count}");
                if (SaveManager.Instance.CurrentSave.PartyDetails.Count == 0)
                {
                    Logger.Info("PartyDetails empty, auto-populating with current PlayerID");
                    var pokemonData = new Godot.Collections.Dictionary
                    {
                        { "ID", (int)PlayerID },
                        { "Level", 5 }
                    };
                    SaveManager.Instance.CurrentSave.PartyDetails[0] = pokemonData;
                }
            }
            else
            {
                Logger.Warning("SaveManager.Instance or CurrentSave is NULL in _Ready");
            }
        }
        private void OnPokemonBackButtonPressed()
        {
            if (CommandMenu != null) CommandMenu.Visible = true;
            if (MoveMenu != null) MoveMenu.Visible = false;
            if (PartyMenu != null) PartyMenu.Visible = false;
            BackButton.Visible = false;
            PokemonBackButton.Visible = false;
        }

        public void UpdateLog(string message)
        {
            Logger.Info($"BATTLE LOG: {message}");
        }

        public async void EndBattle()
        {
            await MessageManager.PlayText("The Opponent Pokemon has fainted! You won!");
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.EndBattle();
                QueueFree();
            }
        }
        public async void RunAway ()
        {
            await MessageManager.PlayText("You ran away!");
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.EndBattle();
                QueueFree();
            }
        }
        private void OnBattleButtonPressed()
        {
            if (CommandMenu != null) CommandMenu.Visible = false;
            if (MoveMenu != null) {MoveMenu.Visible = true;
            
            var button1 = MoveMenu.GetChild(0) as Button;
            button1.Text = _playerPokemon.LearnableMoves[0];
            var button2 = MoveMenu.GetChild(1) as Button;
            button2.Text = _playerPokemon.LearnableMoves[1];
            var button3 = MoveMenu.GetChild(2) as Button;
            button3.Text = _playerPokemon.LearnableMoves[2];
            var button4 = MoveMenu.GetChild(3) as Button;
            button4.Text = _playerPokemon.LearnableMoves[3];
            }
        }

        private void OnBackButtonPressed()
        {
            if (CommandMenu != null) CommandMenu.Visible = true;
            if (MoveMenu != null) MoveMenu.Visible = false;
            if (PartyMenu != null) PartyMenu.Visible = false;
        }

        public int CalculateDamage(PokemonResource attacker, PokemonResource defender, int movePower, int attackerLevel)
        {
            // 1. Calculate the core power based on level
            float levelPart = ((2.0f * attackerLevel) / 5.0f) + 2.0f;
            
            // 2. Get the Attack/Defense ratio
            // (In a full game, you'd check if the move is Physical or Special)
            float adRatio = (float)attacker.BaseAttack / defender.BaseDefense;
            
            // 3. Combine parts
            float baseDamage = ((levelPart * movePower * adRatio) / 50.0f) + 2.0f;

            // 4. Add a bit of randomness (usually between 0.85 and 1.0)
            float randomModifier = (float)GD.RandRange(0.85, 1.0);
            
            return Mathf.FloorToInt(baseDamage * randomModifier);
        }

        private async Task OnMoveSelectedAsync(string moveName)
        {
            UpdateLog($"Player used {moveName}!");
            var Move = PokeBase.LoadMove(moveName);
            
            _oppPokemonHp -= CalculateDamage(_playerPokemon, _oppPokemon, Move.Power, 1);
            Logger.Info("Opp pokemon hp down to:", _oppPokemonHp);
            EnemyHPBar.Value = _oppPokemonHp;
            

            if (_oppPokemonHp <= 0)
            {
                EndBattle();
                return;
            }
            else{
                if (StateMachine != null)
                {
                    StateMachine.ChangeState("EnemyTurnState");
                }
            }
            
        }
        private void OnPokemonMenuButtonPressed()
        {
            Logger.Info("onPokemonButtonPressed called");
            if (PartyMenu != null)
            {
                PartyMenu.Visible = true;
                CommandMenu.Visible = false;
                MoveMenu.Visible = false;
                BackButton.Visible = false;
                PokemonBackButton.Visible = true;
                
                Logger.Info($"PartyMenu Visibility: {PartyMenu.Visible}, Children: {PartyMenu.GetChildCount()}");
                var partyDetails = SaveManager.Instance.CurrentSave?.PartyDetails;
                Logger.Info($"PartyDetails count: {partyDetails?.Count ?? -1}");

                int buttonIdx = 0;
                for (int i = 0; i < PartyMenu.GetChildCount(); i++)
                {
                    var child = PartyMenu.GetChild(i);
                    if (child == PokemonBackButton) continue;

                    if (child is Button btn)
                    {
                        if (partyDetails != null && partyDetails.ContainsKey(buttonIdx))
                        {
                            var pokemonData = partyDetails[buttonIdx].AsGodotDictionary();
                            if (pokemonData != null && pokemonData.ContainsKey("ID"))
                            {
                                int idInt = (int)pokemonData["ID"];
                                PokemonID id = (PokemonID)idInt;
                                btn.Text = id.ToString();
                                btn.Disabled = false;
                                btn.Visible = true;
                                Logger.Info($"Set button {buttonIdx} to {id}");
                            }
                        }
                        else
                        {
                            btn.Text = "---";
                            btn.Disabled = true;
                            // Optionally hide empty slots if that's preferred, but let's keep them visible for now to see if they show up
                            btn.Visible = true; 
                        }
                        buttonIdx++;
                    }
                }
            }
            else
            {
                Logger.Error("PartyMenu is NULL in BattleMain!");
            }
        }
        private void switchPokemon (PokemonResource pokemon){
            _playerPokemon = pokemon;
            PlayerSprite.Texture = pokemon.BackSprite;
            _totalPokemonHp = pokemon.BaseHp;
            PlayerHPBar.MaxValue = _totalPokemonHp;
            PlayerHPBar.Value = _totalPokemonHp;
            PlayerNameLabel.Text = pokemon.Name;
            
        }
    
    }
}
