using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;
using System.Threading.Tasks;

namespace Game.Gameplay
{

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
		[Export] public Texture2D BattleBG;
		[Export] public TextureRect BattleBackground;


		[ExportCategory("Buttons")]
		[Export] public BaseButton BattleButton;
		[Export] public BaseButton RunButton;
		[Export] public BaseButton BackButton;
		[Export] public BaseButton PokemonBackButton;
		[Export] public BaseButton PokemonMenuButton;
		[Export] public TextureButton PokeBackButton;
		[Export] public BaseButton BagMenuButton;
		[Export] public BaseButton BagBackButton;
		[Export] public BaseButton[] MoveButtons;
		[Export] public BaseButton[] PokemonButtons;
		[Export] public BaseButton[] BagButtons;
		[Export] public BaseButton[] PokeballButtons;
		[Export] public BaseButton[] ItemButtons;
		[Export] public BaseButton PokeballBackButton;
		[Export] public RichTextLabel[] PokeButtons;

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
		[ExportGroup("Slot UI Elements")]
		[Export] public RichTextLabel[] NameLabels;
		[Export] public RichTextLabel[] LevelLabels;
		[Export] public TextureProgressBar[] HpBars;
		[Export] public Sprite2D[] Sprites;
		[Export] public Node2D Party;
		private bool _isProcessingTurn = false;
		[ExportCategory("Music")]
		[Export] public AudioStream BackgroundMusic;
		[Export] public AudioStreamPlayer MusicPlayer;
		public override void _Ready()
		{
			if (BackgroundMusic != null && MusicPlayer != null)
			{
				Logger.Info($"Playing music: {BackgroundMusic.ResourcePath}");
				MusicPlayer.Stream = BackgroundMusic;
				MusicPlayer.Play();
			}
			else
			{
				Logger.Info($"Music null: BackgroundMusic={BackgroundMusic == null}, MusicPlayer={MusicPlayer == null}");
			}
			Logger.Info("BattleMain initializing...");

			var currentLevel = SceneManager.GetCurrentLevel();
			if (BattleBackground != null && currentLevel?.battleBackgroundTexture != null)
				BattleBackground.Texture = currentLevel.battleBackgroundTexture;

			var msg = MessageManager.Instance;
			msg.GetParent()?.RemoveChild(msg);
			AddChild(msg);
			msg.Layer = 10;
			
			if (BattleManager.Instance != null)
			{
				PlayerID = BattleManager.Instance.playerPokemonId;
				OpponentID = BattleManager.Instance.opponentPokemonId;
			}

			if (PlayerID == PokemonID.none)
			{
				Logger.Warning("PlayerID was 'none' at runtime. Recovering...");
				PlayerID = BattleManager.Instance?.GetSavedPlayerPokemon() ?? PokemonID.bulbasaur;
			}

			_oppPokemonLevel = BattleManager.Instance != null && BattleManager.Instance.opponentPokemonLevel > 0
				? BattleManager.Instance.opponentPokemonLevel
				: 5;

			if (OpponentNameLabel != null) OpponentNameLabel.Text = OpponentID != PokemonID.none ? $"{OpponentID} Lv.{_oppPokemonLevel}" : "Opponent";

			if (PlayerSprite != null) PlayerSprite.Setup(PlayerID);
			if (OpponentSprite != null) OpponentSprite.Setup(OpponentID);

			Logger.Info($"BattleMain _Ready. PartyMenu: {PartyMenu?.Name ?? "NULL"}, PokemonButtons Count: {PokemonButtons?.Length ?? 0}");
			
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

			if (PlayerNameLabel != null) PlayerNameLabel.Text = $"{PlayerID} Lv.{_playerPokemonLevel}";

			if (_playerPokemonHp <= 0) _playerPokemonHp = _playerPokemon.BaseHp;
			_oppPokemonHp = _oppPokemon.BaseHp;
			PlayerHPBar.MaxValue = _playerPokemon.BaseHp;
			PlayerHPBar.Value = _playerPokemonHp;
			EnemyHPBar.MaxValue = _oppPokemon.BaseHp;
			EnemyHPBar.Value = _oppPokemonHp;
			if (Bag.GetChild(0) is Button pokeballBtn) pokeballBtn.Pressed += () => OnPokeballButtonPressed();
			if (Bag.GetChild(1) is Button expBtn) expBtn.Pressed += () => OnItemButtonPressed();
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
							_ = SafeThrowPokeballAsync(ballType);
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

			if (StateMachine != null)
			{
				StateMachine.StartBattle();
			}

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

		
			if (partyData == null) 
			{
				Game.Core.Logger.Error("Party Data is null!");
				return;
			}

			for (int i = 0; i < partyData.Count; i++)
			{
				if (i >= NameLabels.Length) break;

				var pokemonDict = partyData[i].AsGodotDictionary();
				Game.Core.Logger.Info(pokemonDict);


				var idKey = pokemonDict.ContainsKey("ID") ? "ID" : "Id";
				var pokemonID = (PokemonID)(int)pokemonDict[idKey];
				var pokemonResource = PokeBase.LoadPokemon(pokemonID);

				if (pokemonResource == null)
				{
					GD.PrintErr($"Could not load resource for ID: {pokemonID}");
					continue;
				}

				if (NameLabels[i] != null)
					NameLabels[i].Text = $"[url]{pokemonResource.Name}[/url]";

					NameLabels[i].MetaClicked += (meta) => switchPokemon(pokemonResource);

				if (LevelLabels[i] != null && pokemonDict.ContainsKey("Level"))
					LevelLabels[i].Text =  pokemonDict["Level"].ToString();

				if (HpBars[i] != null && pokemonDict.ContainsKey("CurrentHP"))
				{
					HpBars[i].MaxValue = pokemonResource.BaseHp;
					HpBars[i].Value = pokemonDict["CurrentHP"].AsInt32();
				}
				
				if (Sprites[i] != null)
					Sprites[i].Texture = pokemonResource.FrontSprite; 
			}
			PokeBackButton.Pressed += OnPokemonBackButtonPressed;
		}
		private void OnPokemonBackButtonPressed()
		{
			if (CommandMenu != null) CommandMenu.Visible = true;
			if (MoveMenu != null) MoveMenu.Visible = false;
			if (PartyMenu != null) PartyMenu.Visible = false;
			BackButton.Visible = false;
			PokemonBackButton.Visible = false;
			Party.Visible = false;
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
					await MessageManager.PlayText(null, new string[] { "The Opponent Pokemon has fainted! You won!" });
					AwardExpToActivePokemon(_oppPokemon.BaseExperience);
					break;
				case 2:
					await MessageManager.PlayText(null, new string[] { $"You caught the {_oppPokemon.Name}!" });
					break;
			}
			SaveActivePokemonHp();
			
			var msg = MessageManager.Instance;
			msg.GetParent()?.RemoveChild(msg);
			GetTree().Root.AddChild(msg);
			
			if (BattleManager.Instance != null)
			{
				BattleManager.Instance.EndBattle();
				QueueFree();
			}
		}
		public async void RunAway ()
		{
			var msg = MessageManager.Instance;
			msg.GetParent()?.RemoveChild(msg);
			GetTree().Root.AddChild(msg);


			await MessageManager.PlayText(null, new string[] { "You ran away!" });
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
			if (MoveMenu != null)
			{
				MoveMenu.Visible = true;

				for (int i = 0; i < 4; i++)
				{
					if (i >= _playerPokemon.LearnableMoves.Count) break;

					var btn = MoveMenu.GetChild(i) as Button;
					if (btn == null) continue;

					string moveName = _playerPokemon.LearnableMoves[i];
					btn.Text = moveName;

					var move = PokeBase.LoadMove(moveName);
					if (move != null)
					{
						string typeName = move.PokemonType.ToString().ToLower();
						var texture = ResourceLoader.Load<Texture2D>($"res://assets/ui/move_buttons/{typeName}_button.png");
						if (texture != null)
						{
							var style = new StyleBoxTexture();
							style.Texture = texture;
							btn.AddThemeStyleboxOverride("normal", style);
							btn.AddThemeStyleboxOverride("hover", style);
							btn.AddThemeStyleboxOverride("pressed", style);
							btn.AddThemeColorOverride("font_color", new Color(1, 1, 1)); // white text
						}
					}
				}
			}
		}
		private async void BattleLost()
		{
			await MessageManager.PlayText(null, new string[] { "Your Pokemon has fainted! You blacked out!" });
			SaveActivePokemonHp();

			if (BattleManager.Instance != null)
				BattleManager.Instance.EndBattle();

			var currentLevel = SceneManager.GetCurrentLevel();
			if (currentLevel != null)
				SceneManager.ChangeLevel(levelName: currentLevel.PokemonCenter, trigger: 0);

			QueueFree();
		}
		private void OnBackButtonPressed()
		{
			if (CommandMenu != null) CommandMenu.Visible = true;
			if (MoveMenu != null) MoveMenu.Visible = false;
			if (PartyMenu != null) PartyMenu.Visible = false;
		}

		public (int damage, float multiplier) CalculateDamage(PokemonResource attacker, PokemonResource defender, MoveResource move, int attackerLevel)
		{
			float levelPart = ((2.0f * attackerLevel) / 5.0f) + 2.0f;
			float adRatio = (float)attacker.BaseAttack / defender.BaseDefense;
			float baseDamage = ((levelPart * move.Power * adRatio) / 50.0f) + 2.0f;
			float randomModifier = (float)GD.RandRange(0.85, 1.0);
			float multiplier = TypeChart.GetMultiplier(move.PokemonType, defender.TypeOne);
			
			return (Mathf.FloorToInt(baseDamage * randomModifier * multiplier), multiplier);
		}

		private async Task OnMoveSelectedAsync(string moveName)
		{
			if (_isProcessingTurn) return;
			_isProcessingTurn = true;

			SetMoveButtonsDisabled(true);

			await ExecuteMoveAsync(_playerPokemon, _oppPokemon, moveName, true);

			if (StateMachine != null)
				StateMachine.ChangeState("CheckFaintState");

			SetMoveButtonsDisabled(false);
			_isProcessingTurn = false;
		}

		private void SetMoveButtonsDisabled(bool disabled)
		{
		    if (MoveButtons == null) return;
		    foreach (var btn in MoveButtons)
		        if (btn != null) btn.Disabled = disabled;
		}
				
				

		public async Task ExecuteMoveAsync(PokemonResource attacker, PokemonResource defender, string moveName, bool isPlayerAttacking)
		{
			

			LastTurnWasPlayer = isPlayerAttacking;
			UpdateLog($"{(isPlayerAttacking ? "Player" : "Enemy")} used {moveName}!");
			await MessageManager.PlayText(null, new string[] { $"{(isPlayerAttacking ? _playerPokemon.Name : _oppPokemon.Name)} used {moveName}!" });
			var move = PokeBase.LoadMove(moveName);
			if (move == null) return;
			

			int attackerLevel = isPlayerAttacking ? _playerPokemonLevel : _oppPokemonLevel;
			var (damage, multiplier) = CalculateDamage(attacker, defender, move, attackerLevel);
			string effectMessage = TypeChart.GetEffectivenessMessage(multiplier);
			
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

			if (!string.IsNullOrEmpty(effectMessage))
    			await MessageManager.PlayText(null, new string[] { effectMessage });
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
			_isProcessingTurn = true;
			SetMoveButtonsDisabled(true);
			if (MoveMenu != null) MoveMenu.Visible = false;
			if (CommandMenu != null) CommandMenu.Visible = false;

			if (_oppPokemon == null) return;
			UpdateLog($"Enemy {_oppPokemon.Name} is thinking...");
			await Task.Delay(1500);

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

			if (_playerPokemonHp <= 0)
			{
				int healthyCount = 0;
				foreach (var entry in SaveManager.Instance?.CurrentSave?.PartyDetails)
				{
					var dict = entry.Value.AsGodotDictionary();
					if (dict.ContainsKey("CurrentHP") && dict["CurrentHP"].AsInt32() > 0)
						healthyCount++;
				}

				if (healthyCount == 0)
				{
					BattleLost();
					return;
				}
				else
				{
					await MessageManager.PlayText(null, new string[] { $"{PlayerID} fainted! Choose your next Pokemon!" });
					OnPokemonMenuButtonPressed();
					_isProcessingTurn = false;
					return;
				}
			}

			if (StateMachine != null)
				StateMachine.ChangeState("CheckFaintState");

			if (CommandMenu != null) CommandMenu.Visible = true;
			SetMoveButtonsDisabled(false);
			_isProcessingTurn = false;
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
				if (Bag.GetChild(0) is Button pbBtn) { pbBtn.Text = "Pokeballs"; pbBtn.Visible = true; }
				if (Bag.GetChild(1) is Button expBtn) { expBtn.Text = "Exp"; expBtn.Visible = true; }
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
				PartyMenu.Visible = false;
				CommandMenu.Visible = false;
				MoveMenu.Visible = false;
				BackButton.Visible = false;
				PokemonBackButton.Visible = true;
				Party.Visible = true;
				
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
								bool fainted = pokemonData.ContainsKey("CurrentHP") && pokemonData["CurrentHP"].AsInt32() <= 0;
								btn.Text = fainted ? $"{id} (Lv.{lvl}) - Fainted" : $"{id} (Lv.{lvl})";
								btn.Disabled = fainted;
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
			Party.Visible =false;
			CommandMenu.Visible = true;
		}

		private void OnPartyButtonPressed(int index)
		{
			var partyDetails = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (partyDetails == null || !partyDetails.ContainsKey(index)) return;

			var pokemonData = partyDetails[index].AsGodotDictionary();
			if (pokemonData == null || !pokemonData.ContainsKey("ID")) return;

			if (pokemonData.ContainsKey("CurrentHP") && pokemonData["CurrentHP"].AsInt32() <= 0)
			{
				Logger.Info("Cannot select a fainted Pokemon.");
				return;
			}

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
				Game.Core.Pokeball.Heavy,
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
			await Task.Delay(1000);
			return chance <= catchRate;
		}
		private async Task PokeballthrownAsync (Game.Core.Pokeball ball){
			OpponentSprite.Texture = ResourceLoader.Load<Texture2D>($"res://resources/textures/pokeball_{ball.ToString().ToLowerInvariant()}_closed.tres");
			
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
				OpponentSprite.Texture = ResourceLoader.Load<Texture2D>($"res://resources/textures/pokeball_{ball.ToString().ToLowerInvariant()}_open.tres");
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
				SaveManager.Instance.SaveToDisk();
			}
		}
		private async Task OnSelectPokeballButtonPressedAsync(Game.Core.Pokeball ball){
			await PokeballthrownAsync(ball);
		}

		private async Task SafeThrowPokeballAsync(Game.Core.Pokeball ball)
		{
			try
			{
				await OnSelectPokeballButtonPressedAsync(ball);
			}
			catch (Exception e)
			{
				Logger.Error($"Throwing {ball} pokeball failed: {e}");
			}
		}


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
					await MessageManager.PlayText(null, new string[] { $"{PlayerID} grew to Lv.{currentLevel}!" });
					await CheckEvolution(PlayerID, currentLevel, i);
				}
				
				entry["Level"] = currentLevel;
				entry["Exp"] = currentExp;
				party[i] = entry;
				SaveManager.Instance.SaveToDisk();
				break;
			}
		}
		private async Task CheckEvolution(PokemonID currentID, int newLevel, int partyIndex){
			if (
				!_playerPokemon.CanEvolve
			)
			{
				return;
			}
			var evolutionTarget = _playerPokemon.EvolvesInto;
			for (int i = 0; i < 6; i++)
			{
				PlayerSprite.Visible = false;
				await Task.Delay(200);
				PlayerSprite.Visible = true;
				await Task.Delay(200);
			}
			var party = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (party != null)
			{
				var entry = party[partyIndex].AsGodotDictionary();
				entry["ID"] = (int)evolutionTarget;
				party[partyIndex] = entry;
				SaveManager.Instance.SaveToDisk();
			}
			PlayerID = evolutionTarget;
			_playerPokemon = PokeBase.LoadPokemon(evolutionTarget);
			if (PlayerSprite != null) PlayerSprite.Setup(evolutionTarget);
			if (PlayerNameLabel != null) 
				PlayerNameLabel.Text = $"{evolutionTarget} Lv.{newLevel}";
		} 
		public (string action, int amount, string stat) ParseEffect(string ShortEffect){
			if (string.IsNullOrEmpty(ShortEffect)) return ("", 0, "");

			var parts = ShortEffect.TrimEnd('.').Split(' ');
			
			string action = parts.Length > 0 ? parts[0] : "";
			string stat = parts.Length > 2 ? parts[^1] : "";
			int amount = 0;
			
			foreach (var part in parts)
				if (int.TryParse(part, out int val))
				{ amount = val; break; }

			return (action, amount, stat);
		}

	}
}
