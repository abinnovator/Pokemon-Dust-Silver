using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;
using System.Threading.Tasks;

namespace Game.Gameplay
{
	public partial class TrainerBattleMain : CanvasLayer
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
		[Export] public TrainerBattleStateMachine StateMachine;
		[Export] public TrainerPlayerPokemon PlayerSprite;
		[Export] public TrainerOppPokemon OpponentSprite;
		[Export] public Sprite2D OpponentTrainerSprite;

		[ExportCategory("Enemy Party")]
		[Export] public Godot.Collections.Array<PokemonResource> EnemyParty;

		[ExportCategory("Gym Leader")]
		[Export] public bool IsGymLeader = false;
		[Export] public Game.Core.Badge GymBadge = Game.Core.Badge.BOULDER;

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
		private int _currentEnemyPartyIndex = 0;
		private bool _isProcessingTurn = false;

		[ExportGroup("Slot UI Elements")]
		[Export] public RichTextLabel[] NameLabels;
		[Export] public RichTextLabel[] LevelLabels;
		[Export] public TextureProgressBar[] HpBars;
		[Export] public Sprite2D[] Sprites;
		[Export] public Node2D Party;
		[ExportCategory("Music")]
		[Export] public AudioStream BackgroundMusic;
		[Export] public AudioStreamPlayer MusicPlayer;
		[Export] public ItemList ItemList;


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
			Logger.Info("TrainerBattleMain initializing...");
			var messageManager = MessageManager.Instance;
			messageManager.GetParent().RemoveChild(messageManager);
			AddChild(messageManager);
			messageManager.Layer = 201;

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

			if (OpponentNameLabel != null) OpponentNameLabel.Text = OpponentID != PokemonID.none ? OpponentID.ToString() : "Opponent";

			if (PlayerSprite != null) PlayerSprite.Setup(PlayerID);
			if (OpponentSprite != null) OpponentSprite.Setup(OpponentID);

			if (BattleButton != null) BattleButton.Pressed += () => OnBattleButtonPressed();
			if (RunButton != null) RunButton.Pressed += () => RunAway();
			if (BackButton != null) BackButton.Pressed += () => OnBackButtonPressed();
			if (PokemonMenuButton != null) PokemonMenuButton.Pressed += () => OnPokemonMenuButtonPressed();
			if (PokemonBackButton != null) PokemonBackButton.Pressed += () => OnPokemonBackButtonPressed();
			if (BagMenuButton != null) BagMenuButton.Pressed += () => OnBagMenuButtonPressed();
			if (BagBackButton != null) BagBackButton.Pressed += () => OnBagBackButtonPressed();
			if (PokeballBackButton != null) PokeballBackButton.Pressed += () => OnPokeballBackButtonPressed();

			if (PokemonButtons != null)
			{
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

			var gymConfig = BattleManager.Instance?.CurrentGymConfig;
			if (gymConfig != null)
			{
				IsGymLeader = gymConfig.IsGymLeader;
				GymBadge = gymConfig.Badge;
				EnemyParty = new Godot.Collections.Array<PokemonResource>();
				foreach (var entry in gymConfig.TrainerTeam)
				{
					var pokemon = PokeBase.LoadPokemon(entry.Key);
					if (pokemon != null)
						EnemyParty.Add(pokemon);
				}
				Logger.Info($"Loaded {EnemyParty.Count} Pokemon from gym config for {gymConfig.LeaderName}");
			}
			else
			{
				var trainerConfig = BattleManager.Instance?.CurrentBattleConfig;
				if (trainerConfig != null && trainerConfig.HasBattle)
				{
					IsGymLeader = false;
					EnemyParty = new Godot.Collections.Array<PokemonResource>();
					var pokemon = PokeBase.LoadPokemon(trainerConfig.PokemonID);
					if (pokemon != null)
						EnemyParty.Add(pokemon);

					if (trainerConfig.PokemonLevel > 0)
						_oppPokemonLevel = trainerConfig.PokemonLevel;
				}
			}

			var partyData = SaveManager.Instance?.CurrentSave?.PartyDetails;
			if (partyData != null)
			{
				for (int i = 0; i < partyData.Count; i++)
				{
					var entry = partyData[i].AsGodotDictionary();
					if (entry != null && entry.ContainsKey("ID") && (PokemonID)(int)entry["ID"] == PlayerID)
					{
						_playerPokemonLevel = entry.ContainsKey("Level") ? (int)entry["Level"] : 5;
						if (entry.ContainsKey("CurrentHP"))
							_playerPokemonHp = (int)entry["CurrentHP"];
						break;
					}
				}
			}

			if (_playerPokemonLevel <= 0) _playerPokemonLevel = 5;
			if (_oppPokemonLevel <= 0) _oppPokemonLevel = 5;

			if (PlayerNameLabel != null) PlayerNameLabel.Text = $"{PlayerID} Lv.{_playerPokemonLevel}";

			if (_playerPokemonHp <= 0 && _playerPokemon != null) _playerPokemonHp = _playerPokemon.BaseHp;
			if (_oppPokemon != null)
			{
				_oppPokemonHp = _oppPokemon.BaseHp;
				if (PlayerHPBar != null) { PlayerHPBar.MaxValue = _playerPokemon?.BaseHp ?? 1; PlayerHPBar.Value = _playerPokemonHp; }
				if (EnemyHPBar != null) { EnemyHPBar.MaxValue = _oppPokemon.BaseHp; EnemyHPBar.Value = _oppPokemonHp; }
			}

			if (Bag != null)
			{
				if (Bag.GetChild(0) is Button expBtn) expBtn.Pressed += () => OnItemButtonPressed();
				if (Bag.GetChild(1) is Button pokeballBtn) pokeballBtn.Pressed += () => OnPokeballButtonPressed();
			}

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
				StateMachine.StartBattle();

			if (SaveManager.Instance?.CurrentSave != null)
			{
				if (SaveManager.Instance.CurrentSave.PartyDetails.Count == 0)
				{
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
				Logger.Error("Party Data is null!");
				return;
			}

			for (int i = 0; i < partyData.Count; i++)
			{
				if (i >= NameLabels.Length) break;

				var pokemonDict = partyData[i].AsGodotDictionary();
				var idKey = pokemonDict.ContainsKey("ID") ? "ID" : "Id";
				var pokemonID = (PokemonID)(int)pokemonDict[idKey];
				var pokemonResource = PokeBase.LoadPokemon(pokemonID);

				if (pokemonResource == null)
				{
					GD.PrintErr($"Could not load resource for ID: {pokemonID}");
					continue;
				}

				if (NameLabels[i] != null)
				{
					NameLabels[i].Text = $"[url]{pokemonResource.Name}[/url]";
					NameLabels[i].MetaClicked += (meta) => switchPokemon(pokemonResource);
				}

				if (LevelLabels[i] != null && pokemonDict.ContainsKey("Level"))
					LevelLabels[i].Text = pokemonDict["Level"].ToString();

				if (HpBars[i] != null && pokemonDict.ContainsKey("CurrentHP"))
				{
					HpBars[i].MaxValue = pokemonResource.BaseHp;
					HpBars[i].Value = pokemonDict["CurrentHP"].AsInt32();
				}

				if (Sprites[i] != null)
					Sprites[i].Texture = pokemonResource.FrontSprite;
			}

			if (OpponentTrainerSprite != null && gymConfig != null)
			{
				Logger.Info($"TrainerAtlas: {gymConfig.TrainerAtlas}");
				OpponentTrainerSprite.Texture = gymConfig.TrainerAtlas;
			}
		}

		private void OnPokemonBackButtonPressed()
		{
			if (CommandMenu != null) CommandMenu.Visible = true;
			if (MoveMenu != null) MoveMenu.Visible = false;
			if (PartyMenu != null) PartyMenu.Visible = false;
			if (BackButton != null) BackButton.Visible = false;
			if (PokemonBackButton != null) PokemonBackButton.Visible = false;
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

		public async Task PlayVictoryDialogueAsync()
		{
			var gymConfig = BattleManager.Instance?.CurrentGymConfig;
			if (gymConfig != null)
			{
				if (gymConfig.IsGymLeader)
				{
					var badges = SaveManager.Instance.CurrentSave.Badges;
					if (!badges.Contains(gymConfig.Badge))
					{
						badges.Add(gymConfig.Badge);
						SaveManager.Instance.SaveToDisk();
					}
				}

				var messages = gymConfig.VictoryMessages;
				if (messages == null || messages.Count == 0) return;

				if (!SaveManager.Instance.CurrentSave.DefeatedTrainers.Contains(gymConfig.LeaderName))
					SaveManager.Instance.CurrentSave.DefeatedTrainers.Add(gymConfig.LeaderName);

				SaveManager.Instance.SaveToDisk();

				foreach (var line in messages)
					await MessageManager.PlayText(line);
			}
			else
			{
				var trainerConfig = BattleManager.Instance?.CurrentBattleConfig;
				if (trainerConfig != null && trainerConfig.HasBattle)
				{
					if (!string.IsNullOrEmpty(trainerConfig.TrainerID) &&
						!SaveManager.Instance.CurrentSave.DefeatedTrainers.Contains(trainerConfig.TrainerID))
					{
						SaveManager.Instance.CurrentSave.DefeatedTrainers.Add(trainerConfig.TrainerID);
						SaveManager.Instance.SaveToDisk();
					}

					var message = trainerConfig.AfterBattleMessage;
					if (!string.IsNullOrEmpty(message))
						await MessageManager.PlayText(message);
				}
			}
		}

		public async void EndBattle(int type)
		{
			switch (type)
			{
				case 1:
					await MessageManager.PlayText("The Opponent Pokemon has fainted! You won!");
					AwardExpToActivePokemon(_oppPokemon.BaseExperience);
					await PlayVictoryDialogueAsync();
					break;
			}
			SaveActivePokemonHp();
			if (BattleManager.Instance != null)
			{
				BattleManager.Instance.EndBattle();
				QueueFree();
			}
			GameManager.IsPlayerMovementLocked = true;
			GetTree().CreateTimer(0.3f).Timeout += () => 
			{
				GameManager.IsPlayerMovementLocked = false;
			};
		}

		public async void RunAway()
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
			if (MoveMenu != null)
			{
				MoveMenu.Visible = true;
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

		private async void BattleLost()
		{
			await MessageManager.PlayText("Your Pokemon has fainted! You lost!");
			SaveActivePokemonHp();

			var gymConfig = BattleManager.Instance?.CurrentGymConfig;

			if (gymConfig != null)
			{
				var messages = gymConfig.DefeatMessages;
				if (messages != null && messages.Count > 0)
				{
					foreach (var line in messages)
						await MessageManager.PlayText(line);
				}
			}

			if (BattleManager.Instance != null)
			{
				BattleManager.Instance.EndBattle();
			}

			if (gymConfig != null)
			{
				SceneManager.ChangeLevel(levelName: gymConfig.LastPokemonCenter, trigger: 0);
			}

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
			var move = PokeBase.LoadMove(moveName);
			if (move == null) return;
			await MessageManager.PlayText($"{(isPlayerAttacking ? _playerPokemon.Name : _oppPokemon.Name)} used {moveName}!");

			int attackerLevel = isPlayerAttacking ? _playerPokemonLevel : _oppPokemonLevel;
			var (damage, multiplier) = CalculateDamage(attacker, defender, move, attackerLevel);
			string effectMessage = TypeChart.GetEffectivenessMessage(multiplier);


			if (isPlayerAttacking)
			{
				_oppPokemonHp = Math.Max(0, _oppPokemonHp - damage);
				if (EnemyHPBar != null) EnemyHPBar.Value = _oppPokemonHp;
				if (OpponentSprite != null) await PlayFlickerAsync(OpponentSprite);
				Logger.Info($"Enemy HP down to: {_oppPokemonHp}");
			}
			else
			{
				_playerPokemonHp = Math.Max(0, _playerPokemonHp - damage);
				if (PlayerHPBar != null) PlayerHPBar.Value = _playerPokemonHp;
				if (PlayerSprite != null) await PlayFlickerAsync(PlayerSprite);
				Logger.Info($"Player HP down to: {_playerPokemonHp}");
			}
			if (!string.IsNullOrEmpty(effectMessage))
				await MessageManager.PlayText(effectMessage);

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

		public async Task<bool> TrySendNextEnemyPokemon()
		{
			if (_oppPokemonHp > 0) return false;

			if (EnemyParty != null)
			{
				for (int i = _currentEnemyPartyIndex + 1; i < EnemyParty.Count; i++)
				{
					var next = EnemyParty[i];
					if (next == null) continue;

					_currentEnemyPartyIndex = i;
					_oppPokemon = next;
					_oppPokemonHp = next.BaseHp;

					if (EnemyHPBar != null) { EnemyHPBar.MaxValue = next.BaseHp; EnemyHPBar.Value = _oppPokemonHp; }
					if (OpponentNameLabel != null) OpponentNameLabel.Text = next.Name;
					if (OpponentSprite != null) OpponentSprite.Setup((PokemonID)next.Id);

					await MessageManager.PlayText($"Opponent sent out {next.Name}!");
					return true;
				}
			}

			return false;
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
				SaveActivePokemonHp();

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
					await MessageManager.PlayText($"{PlayerID} fainted! Choose your next Pokemon!");
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
			if (Bag != null)
			{
				Bag.Visible = true;
				if (CommandMenu != null) CommandMenu.Visible = false;
				if (MoveMenu != null) MoveMenu.Visible = false;
				if (PartyMenu != null) PartyMenu.Visible = false;
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
		}

		private void OnPokemonMenuButtonPressed()
		{
			if (PartyMenu != null)
			{
				PartyMenu.Visible = false;
				CommandMenu.Visible = false;
				MoveMenu.Visible = false;
				BackButton.Visible = false;
				PokemonBackButton.Visible = true;
				Party.Visible = true;

				var partyDetails = SaveManager.Instance.CurrentSave?.PartyDetails;

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
							}
						}
						else
						{
							btn.Text = "---";
							btn.Disabled = true;
							btn.Visible = true;
						}
						buttonIdx++;
					}
				}
			}
			else
			{
				Logger.Error("PartyMenu is NULL in TrainerBattleMain!");
			}
		}

		private void switchPokemon(PokemonResource pokemon)
		{
			_playerPokemon = pokemon;
			PlayerSprite.Texture = pokemon.BackSprite;
			if (PlayerHPBar != null) { PlayerHPBar.MaxValue = pokemon.BaseHp; PlayerHPBar.Value = _playerPokemonHp; }
			if (PlayerNameLabel != null) PlayerNameLabel.Text = $"{pokemon.Name} Lv.{_playerPokemonLevel}";
			Party.Visible = false;
			CommandMenu.Visible = true;
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

			SaveActivePokemonHp();

			var pokemon = PokeBase.LoadPokemon(id);
			if (pokemon == null) return;

			_playerPokemonHp = pokemonData.ContainsKey("CurrentHP")
				? (int)pokemonData["CurrentHP"]
				: pokemon.BaseHp;

			_playerPokemonLevel = pokemonData.ContainsKey("Level") ? (int)pokemonData["Level"] : 5;

			PlayerID = id;
			switchPokemon(pokemon);
			Logger.Info($"Switched active pokemon to {id}");

			OnPokemonBackButtonPressed();
		}

		private void OnPokeballButtonPressed()
		{
			if (Pokeballs != null)
			{
				Pokeballs.Visible = true;
				if (Bag != null) Bag.Visible = false;
				PopulatePokeballItems();
			}
		}

		private void OnItemButtonPressed()
		{
			if (Items != null)
			{
				Items.Visible = true;
				if (Bag != null) Bag.Visible = false;
			}
		}

		private void OnPokeballBackButtonPressed()
		{
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

			var counts = new System.Collections.Generic.Dictionary<Game.Core.Pokeball, int>();
			foreach (var ball in save.Pokeballs)
			{
				if (counts.ContainsKey(ball)) counts[ball]++;
				else counts[ball] = 1;
			}

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
