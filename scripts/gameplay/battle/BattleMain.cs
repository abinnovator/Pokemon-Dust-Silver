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
		[Export] public Control Bag;
		[Export] public Control Pokeballs;
		[Export] public Control Items;


		[ExportCategory("Buttons")]
		[Export] public BaseButton BattleButton;
		[Export] public BaseButton RunButton;
		[Export] public BaseButton BackButton;
		[Export] public BaseButton PokemonBackButton;
		[Export] public BaseButton PokemonMenuButton;
		[Export] public BaseButton BagMenuButton;
		[Export] public BaseButton BagBackButton;
		[Export] public BaseButton[] MoveButtons;
		[Export] public BaseButton[] PokemonButtons;
		[Export] public BaseButton[] BagButtons;
		[Export] public BaseButton[] PokeballButtons;
		[Export] public BaseButton[] ItemButtons;
		[Export] public BaseButton PokeballBackButton;

		[ExportCategory("Systems")]
		[Export] public BattleStateMachine StateMachine;
		[Export] public PlayerPokemon PlayerSprite;
		[Export] public OppPokemon OpponentSprite;

		public PokemonID PlayerID { get; set; }
		public PokemonID OpponentID { get; private set; }
		private PokemonResource _playerPokemon;
		private PokemonResource _oppPokemon;
		private int _playerPokemonHp;
		private int _oppPokemonHp;
		public int PlayerHP => _playerPokemonHp;
		public int OpponentHP => _oppPokemonHp;
		public bool LastTurnWasPlayer { get; private set; }
		private int _oppPokemonLevel;
		private int _playerPokemonLevel;
		private AudioStreamPlayer _audioStreamPlayer;
		public override void _Ready()
		{
			 _audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
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

			// Basic UI Setup (player name label updated after level loads below)
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
			if (BagMenuButton != null) BagMenuButton.Pressed += () => OnBagMenuButtonPressed();
			if (BagBackButton != null) BagBackButton.Pressed += () => OnBagBackButtonPressed();
			if (PokeballBackButton != null) PokeballBackButton.Pressed += () => onPokeballBackButtonPressed();
			
			if (PokemonButtons != null)
			{
				Logger.Info($"Connecting {PokemonButtons.Length} Party Slot Buttons");
				for (int i = 0; i < PokemonButtons.Length; i++)
				{
					int capturedIndex = i;
					var button = PokemonButtons[capturedIndex];
					if (button != null)
						button.Pressed += () => OnPartyButtonPressed(capturedIndex);
					else
						Logger.Warning("Found NULL button in PokemonButtons array");
				}
			}
			_playerPokemon = PokeBase.LoadPokemon(PlayerID);
			_oppPokemon = PokeBase.LoadPokemon(OpponentID);

			// Load the active player pokemon's level from save data
			var partyData = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (partyData != null)
			{
				for (int i = 0; i < partyData.Count; i++)
				{
					var entry = partyData[i].AsGodotDictionary();
					if (entry != null && entry.ContainsKey("ID") && (PokemonID)(int)entry["ID"] == PlayerID)
					{
						_playerPokemonLevel = entry.ContainsKey("Level") ? (int)entry["Level"] : 5;
						// Restore persisted HP; fall back to full HP if not saved yet
						if (entry.ContainsKey("CurrentHP"))
							_playerPokemonHp = (int)entry["CurrentHP"];
						break;
					}
				}
			}
			if (_playerPokemonLevel <= 0) _playerPokemonLevel = 5;
			_oppPokemonLevel = 5;

			// Update name label now that level is known
			if (PlayerNameLabel != null) PlayerNameLabel.Text = $"{PlayerID} Lv.{_playerPokemonLevel}";

			// Use persisted HP if loaded above; otherwise start at full HP
			if (_playerPokemonHp <= 0) _playerPokemonHp = _playerPokemon.BaseHp;
			_oppPokemonHp = _oppPokemon.BaseHp;
			PlayerHPBar.MaxValue = _playerPokemon.BaseHp;
			PlayerHPBar.Value = _playerPokemonHp;
			EnemyHPBar.MaxValue = _oppPokemon.BaseHp;
			EnemyHPBar.Value = _oppPokemonHp;
			if (Bag.GetChild(0) is Button expBtn) expBtn.Pressed += () => OnItemButtonPressed();
			if (Bag.GetChild(1) is Button pokeballBtn) pokeballBtn.Pressed += () => OnPokeballButtonPressed();
			for (int i = 0; i < PokeballButtons.Length; i++)
			{
				var child = PokeballButtons[i];
				if (child is Button btn)
				{
					btn.Pressed += () =>
					{
						Logger.Info($"Pokeball button {btn.Name} pressed");
						// Parse the button name to get the Pokeball enum value
						if (Enum.TryParse<Game.Core.Pokeball>(btn.Name.ToString(), true, out var ballType))
						{
							_ = OnSelectPokeballButtonPressedAsync(ballType);
						}
					};
				}
			}

			Logger.Info($"Player Pokemon Learnable Moves: {string.Join(", ", _playerPokemon.LearnableMoves)}");
			
			if (MoveButtons != null)
			{
				foreach (var btn in MoveButtons)
				{
					if (btn != null)
					{
						btn.Pressed += async () => 
						{
							string moveName = (btn as Button)?.Text ?? btn.Name.ToString();
							await OnMoveSelectedAsync(moveName);
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
			// for (int i=0; i<PokemonButtons.Length; i++)
			// {
			//     PokemonButtons[i].Pressed += () => switchPokemon();
			// }
		}
		private void OnPokemonBackButtonPressed()
		{
			if (CommandMenu != null) CommandMenu.Visible = true;
			if (MoveMenu != null) MoveMenu.Visible = false;
			if (PartyMenu != null) PartyMenu.Visible = false;
			BackButton.Visible = false;
			PokemonBackButton.Visible = false;
		}
		private void OnBagBackButtonPressed()
		{
			if (CommandMenu != null) CommandMenu.Visible = true;
			if (Bag != null) Bag.Visible = false;
			if (BackButton != null) BackButton.Visible = true;
			if (BagBackButton != null) BagBackButton.Visible = false;

		}

		public void UpdateLog(string message)
		{
			Logger.Info($"BATTLE LOG: {message}");
		}

		public async void EndBattle(int type)
		{
			switch (type)
			{
				case 1:
					await MessageManager.PlayText("The Opponent Pokemon has fainted! You won!");
					AwardExpToActivePokemon(_oppPokemon.BaseExperience);
					break;
				case 2:
					await MessageManager.PlayText($"You caught the {_oppPokemon.Name}!");
					break;
			}
			SaveActivePokemonHp();
			if (BattleManager.Instance != null)
			{
				BattleManager.Instance.EndBattle();
				QueueFree();
			}
		}
		public async void RunAway ()
		{
			await MessageManager.PlayText("You ran away!");
			SaveActivePokemonHp();
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
		private async void BattleLost (){
			await MessageManager.PlayText("Your Pokemon has fainted! You lost!");
			SaveActivePokemonHp(); // saves HP as 0 so it's clearly fainted
			if (BattleManager.Instance != null)
			{
				BattleManager.Instance.EndBattle();
				QueueFree();
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
			await ExecuteMoveAsync(_playerPokemon, _oppPokemon, moveName, true);
			
			if (StateMachine != null)
			{
				StateMachine.ChangeState("CheckFaintState");
			}
		}

		public async Task ExecuteMoveAsync(PokemonResource attacker, PokemonResource defender, string moveName, bool isPlayerAttacking)
		{
			LastTurnWasPlayer = isPlayerAttacking;
			UpdateLog($"{(isPlayerAttacking ? "Player" : "Enemy")} used {moveName}!");
			var move = PokeBase.LoadMove(moveName);
			if (move == null) return;

			int attackerLevel = isPlayerAttacking ? _playerPokemonLevel : _oppPokemonLevel;
			int damage = CalculateDamage(attacker, defender, move.Power, attackerLevel);
			
			if (isPlayerAttacking)
			{
				_oppPokemonHp = Math.Max(0, _oppPokemonHp - damage);
				EnemyHPBar.Value = _oppPokemonHp;
				if (OpponentSprite != null) await PlayFlickerAsync(OpponentSprite);
				Logger.Info($"Enemy HP down to: {_oppPokemonHp}");
			}
			else
			{
				_playerPokemonHp = Math.Max(0, _playerPokemonHp - damage);
				PlayerHPBar.Value = _playerPokemonHp;
				if (PlayerSprite != null) await PlayFlickerAsync(PlayerSprite);
				Logger.Info($"Player HP down to: {_playerPokemonHp}");
			}

			await Task.Delay(500);
		}

		private async Task PlayFlickerAsync(CanvasItem sprite)
		{
			for (int i = 0; i < 4; i++)
			{
				sprite.Visible = false;
				await Task.Delay(100);
				sprite.Visible = true;
				await Task.Delay(100);
			}
		}

		public async Task ExecuteEnemyTurnAsync()
		{
			if (_oppPokemon == null) return;
			UpdateLog($"Enemy {_oppPokemon.Name} is thinking...");
			await Task.Delay(1500);

			// Simple AI: Pick move with highest power
			string bestMove = _oppPokemon.LearnableMoves.Count > 0 ? _oppPokemon.LearnableMoves[0] : "Tackle";
			int maxPower = -1;

			foreach (var moveName in _oppPokemon.LearnableMoves)
			{
				var move = PokeBase.LoadMove(moveName);
				if (move != null && move.Power > maxPower)
				{
					maxPower = move.Power;
					bestMove = moveName;
				}
			}

			await ExecuteMoveAsync(_oppPokemon, _playerPokemon, bestMove, false);
			if (_playerPokemonHp<=0){
				BattleLost();
			}
			
			if (StateMachine != null)
			{
				StateMachine.ChangeState("CheckFaintState");
			}
		}
		private void OnBagMenuButtonPressed()
		{
			Logger.Info("OnBagMenuButtonPressed called");
			if (Bag != null)
			{
				Bag.Visible = true;
				CommandMenu.Visible = false;
				MoveMenu.Visible = false;
				PartyMenu.Visible = false;
				
				if (BackButton != null) BackButton.Visible = false;
				if (BagBackButton != null) BagBackButton.Visible = true;
				if (Bag.GetChild(0) is Button expBtn) { expBtn.Text = "Exp"; expBtn.Visible = true; }
				if (Bag.GetChild(1) is Button pbBtn) { pbBtn.Text = "Pokeballs"; pbBtn.Visible = true; }
				if (Bag.GetChild(2) is Button backBtn) { backBtn.Text = "Back"; backBtn.Visible = true; }
				
				
				PopulateBagItems();
			}
		}

		private void PopulateBagItems()
		{
			var save = SaveManager.Instance?.CurrentSave;
			if (save == null) return;

			Logger.Info($"Populating Bag with {save.Pokeballs.Count} items");
			
			// for (int i = 0; i < BagButtons.Length; i++)
			// {
			//     var btn = BagButtons[i];
			//     if (btn == null) continue;

			//     if (i < save.Pokeballs.Count)
			//     {
			//         var ball = save.Pokeballs[i];
			//         if (btn is Button b) b.Text = ball.ToString();
			//         btn.Visible = true;
			//         btn.Disabled = false;
			//     }
			//     else
			//     {
			//         if (btn is Button b) b.Text = "---";
			//         btn.Visible = true; 
			//         btn.Disabled = true;
			//     }
			// }
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
								int lvl = pokemonData.ContainsKey("Level") ? (int)pokemonData["Level"] : 5;
								btn.Text = $"{id} (Lv.{lvl})";
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
		private void switchPokemon(PokemonResource pokemon)
		{
			_playerPokemon = pokemon;
			PlayerSprite.Texture = pokemon.BackSprite;
			PlayerHPBar.MaxValue = pokemon.BaseHp;
			PlayerHPBar.Value = _playerPokemonHp;
			PlayerNameLabel.Text = $"{pokemon.Name} Lv.{_playerPokemonLevel}";
		}

		private void OnPartyButtonPressed(int index)
		{
			var partyDetails = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (partyDetails == null || !partyDetails.ContainsKey(index)) return;

			var pokemonData = partyDetails[index].AsGodotDictionary();
			if (pokemonData == null || !pokemonData.ContainsKey("ID")) return;

			int idInt = (int)pokemonData["ID"];
			PokemonID id = (PokemonID)idInt;

			if (id == PlayerID)
			{
				Logger.Info($"{id} is already the active pokemon.");
				OnPokemonBackButtonPressed();
				return;
			}

			var pokemon = PokeBase.LoadPokemon(id);
			if (pokemon == null) return;

			// Restore saved HP if available, otherwise use full HP
			_playerPokemonHp = pokemonData.ContainsKey("CurrentHP")
				? (int)pokemonData["CurrentHP"]
				: pokemon.BaseHp;

			_playerPokemonLevel = pokemonData.ContainsKey("Level") ? (int)pokemonData["Level"] : 5;

			PlayerID = id;
			switchPokemon(pokemon);
			Logger.Info($"Switched active pokemon to {id}");

			OnPokemonBackButtonPressed();
		}
		private void OnPokeballButtonPressed(){
			Logger.Info("OnPokeballButtonPressed called");
			if (Pokeballs != null)
			{
				Pokeballs.Visible = true;
				Bag.Visible = false;
				PopulatePokeballItems();
			}
		}
		private void OnItemButtonPressed(){
			Logger.Info("OnItemButtonPressed (Exp) called");
			if (Items != null)
			{
				Items.Visible = true;
				Bag.Visible = false;
			}
		}
		private void onPokeballBackButtonPressed(){
			if (CommandMenu != null) CommandMenu.Visible = false;
			if (MoveMenu != null) MoveMenu.Visible = false;
			if (PartyMenu != null) PartyMenu.Visible = false;
			if (Pokeballs != null) Pokeballs.Visible = false;
			if (Items != null) Items.Visible = false;
			if (Bag != null) Bag.Visible = true;
			if (BagBackButton != null) BagBackButton.Visible = true;
		}

		private void PopulatePokeballItems()
		{
			var save = SaveManager.Instance?.CurrentSave;
			if (save == null) return;

			// Group balls by type for counting
			var counts = new System.Collections.Generic.Dictionary<Game.Core.Pokeball, int>();
			foreach (var ball in save.Pokeballs)
			{
				if (counts.ContainsKey(ball)) counts[ball]++;
				else counts[ball] = 1;
			}

			// Define the order of Pokeballs to display (matching button indices)
			Game.Core.Pokeball[] displayOrder = {
				Game.Core.Pokeball.Normal,
				Game.Core.Pokeball.Great,
				Game.Core.Pokeball.Ultra,
				Game.Core.Pokeball.Master
			};

			for (int i = 0; i < PokeballButtons.Length; i++)
			{
				if (PokeballButtons[i] is Button btn)
				{
					if (i < displayOrder.Length)
					{
						var type = displayOrder[i];
						int count = counts.ContainsKey(type) ? counts[type] : 0;
						btn.Text = $"{type} (x{count})";
						btn.Disabled = count == 0;
						btn.Visible = true;
					}
					else
					{
						btn.Visible = false;
					}
				}
			}
		}
		private async Task<bool> AttemptCatch(Game.Core.Pokeball ball){
			int chance = GD.RandRange(0, 255);
			var ballResource = PokeBase.LoadPokeball(ball);
			float catchMultiplier = ballResource != null ? ballResource.CatchRate : 1.0f;
			int catchRate = (int)(((3f * _oppPokemon.BaseHp - 2f * _oppPokemonHp) * catchMultiplier) / (3f * _oppPokemon.BaseHp) * 255);
			await Task.Delay(1000); // animation delay
			return chance <= catchRate;
		}
		private async Task PokeballthrownAsync (Game.Core.Pokeball ball){
			OpponentSprite.Texture = ResourceLoader.Load<Texture2D>($"res://resources/textures/pokeball_{ball}_closed.tres");
			
			await AttemptCatch(ball);
			SaveManager.Instance.CurrentSave.Pokeballs.Remove(ball);
			SaveManager.Instance.SaveToDisk();
			if (await AttemptCatch(ball))
			{
				SavePokemonToParty((PokemonID)_oppPokemon.Id);
				Logger.Info("Pokemon caught!");
				_audioStreamPlayer.Play();
				EndBattle(2);
				QueueFree();
			}else{
				OpponentSprite.Texture = ResourceLoader.Load<Texture2D>($"res://resources/textures/pokeball_{ball}_open.tres");
				await Task.Delay(1000); // animation delay
				OpponentSprite.Texture = _oppPokemon.FrontSprite;
			}
		}
		private void SavePokemonToParty(PokemonID id)
		{
			var party = SaveManager.Instance.CurrentSave.PartyDetails;
			if (party.Count < 6)
			{
				var newPokeData = new Godot.Collections.Dictionary {
					{ "ID", (int)id },
					{ "Level", 5 },
					{ "CurrentHP", _oppPokemonHp } 
				};
				party[party.Count] = newPokeData;
				SaveManager.Instance.SaveToDisk(); // Use your new X-key save logic!
			}
		}
		private async Task OnSelectPokeballButtonPressedAsync(Game.Core.Pokeball ball){
			await PokeballthrownAsync(ball);
		}

		/// <summary>
		/// Persists the active player Pokemon's current HP to the save data.
		/// </summary>
		private void SaveActivePokemonHp()
		{
			var party = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (party == null) return;

			for (int i = 0; i < party.Count; i++)
			{
				var entry = party[i].AsGodotDictionary();
				if (entry == null || !entry.ContainsKey("ID")) continue;
				if ((PokemonID)(int)entry["ID"] != PlayerID) continue;

				entry["CurrentHP"] = _playerPokemonHp;
				party[i] = entry;
				SaveManager.Instance.SaveToDisk();
				break;
			}
		}

		/// <summary>
		/// Awards EXP to the active player Pokemon after defeating an opponent.
		/// Levels up the Pokemon if enough EXP has been accumulated (level * 10 per level).
		/// </summary>
		private async void AwardExpToActivePokemon(int baseExp)
		{
			var party = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (party == null) return;

			for (int i = 0; i < party.Count; i++)
			{
				var entry = party[i].AsGodotDictionary();
				if (entry == null || !entry.ContainsKey("ID")) continue;
				if ((PokemonID)(int)entry["ID"] != PlayerID) continue;

				int currentLevel = entry.ContainsKey("Level") ? (int)entry["Level"] : 5;
				int currentExp = entry.ContainsKey("Exp") ? (int)entry["Exp"] : 0;
				int expGained = Math.Max(1, baseExp / 5);
				currentExp += expGained;

				int expThreshold = currentLevel * 10;
				if (currentExp >= expThreshold && currentLevel < 100)
				{
					currentLevel++;
					currentExp -= expThreshold;
					_playerPokemonLevel = currentLevel;
					if (PlayerNameLabel != null)
						PlayerNameLabel.Text = $"{PlayerID} Lv.{_playerPokemonLevel}";
					await MessageManager.PlayText($"{PlayerID} grew to Lv.{currentLevel}!");
				}

				entry["Level"] = currentLevel;
				entry["Exp"] = currentExp;
				party[i] = entry;
				SaveManager.Instance.SaveToDisk();
				break;
			}
		}
	}
}
