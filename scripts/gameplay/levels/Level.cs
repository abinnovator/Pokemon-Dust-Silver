using Godot;
using System;
using Game.Core;
using Logger = Game.Core.Logger;
using System.Collections.Generic;
using Godot.Collections;
using System.Threading.Tasks;

namespace Game.Gameplay
{
	public partial class Level : Node2D
	{
		[ExportCategory("Level Basics")]
		[Export]
		public LevelName LevelName;

		[Export(PropertyHint.Range, "0,100")]
		public int encounterRate;

		[Export]
		public string[] wildPokemonList = { "Pidgey", "Rattata" };
		[ExportCategory("Camera Limits")]
		[Export]
		public int top;
		[Export]
		public int bottom;
		[Export]
		public int left;
		[Export]
		public int right;
		[ExportCategory("Miscellaneous")]
		[Export] public LevelName PokemonCenter;
		[Export] public AudioStream BackgroundMusic;
		[Export] public AudioStreamPlayer MusicPlayer;
		[Export] public ItemResource[] itemsOnGround = [];
		[Export(PropertyHint.Range, "0,100")] public float itemEncounterRate;
		[Export] public Texture2D battleBackgroundTexture;

		public readonly HashSet<Vector2> reservedTiles = [];
		public AStarGrid2D Grid;
		public Vector2 TargetPosition = Vector2.Zero;
		public Array<Vector2> CurrentControlPoints = [];

		public override void _Ready()
		{
			Logger.Info($"Level {LevelName} loaded");
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
		}
		public override void _Process(double delta)
		{
			if (Grid == null && GameManager.GetPlayer() != null){
				SetupGrid();
			}

			if (Grid != null ){
				QueueRedraw();
			}
		}

		public void SetupGrid(){
			Grid = new(){
				Region = new Rect2I(0,0, right, bottom),
				CellSize = new Vector2(Globals.GridSize, Globals.GridSize),
				DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
				DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
				DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never
				
			};

			Grid.Update();

			var mapHeight = bottom / Globals.GridSize;
			var mapWidth = right / Globals.GridSize;

			for (int y = 0; y < mapHeight; y++){
				for (int x = 0; x < mapWidth; x++){
					Vector2I cell = new(x,y);
					Vector2 worldPosition = new(x* Globals.GridSize, y * Globals.GridSize);

					var (_, collisions) = GameManager.GetPlayer().GetNode<CharacterMovement>("Movement").GetTargetColliders(worldPosition);
					foreach (var collision in collisions){
						var collider = (Node)(GodotObject)collision["collider"];
						var colliderType = collider.GetType().Name;
						if (colliderType == "TallGrass" || colliderType == "Player"){
							continue;
						}
						if (colliderType == "Npc"){
							switch (((Npc)collider).NpcInputConfig.NpcMovementType){
								case NpcMovementType.Patrol:
									continue;
								case NpcMovementType.Wander:
									continue;				
							}
						}
						Grid.SetPointSolid(cell,true);

						
					}
				}
			}

		}
		public override void _Draw(){
			if (Grid == null)
			{
				return;
			}
			var mapHeight = bottom / Globals.GridSize;
			var mapWidth = right / Globals.GridSize;

			for (int y = 0; y < mapHeight; y++){
				for (int x = 0; x < mapWidth; x++){
					Vector2I cell = new(x,y);
					Vector2 worldPosition = new(x* Globals.GridSize, y * Globals.GridSize);

					var color = Grid.IsPointSolid(cell) ? new Color(1,0,0,0.3f) : new Color(0,1,0,0.3f);
					DrawRect(new Rect2(worldPosition, Grid.CellSize), color, filled: true);
					
				}
			}
			foreach (var point in CurrentControlPoints){
				DrawRect(new Rect2(point, Grid.CellSize), Colors.Black, filled: true);
			}
			if (TargetPosition != Vector2.Zero){
				DrawRect(new Rect2(TargetPosition, Grid.CellSize), Colors.Cyan, filled: true);
			}
		}
		public bool ReservedTile(Vector2 position)
		{
			if (reservedTiles.Contains(position)){
				return false;
			}
			reservedTiles.Add(position);
			return true;
		}
		public bool IsTileFree(Vector2 position)
		{
			return !reservedTiles.Contains(position);
		}
		public void ReleaseTile(Vector2 position)
		{
			reservedTiles.Remove(position);
		}
		public async Task CalculateItemEncounterChance()
		{
			if (itemsOnGround == null || itemsOnGround.Length == 0) return;

			int chance = Globals.GetRandomNumberGenerator().RandiRange(0, 100);
			if (chance > itemEncounterRate) return;

			int index = Globals.GetRandomNumberGenerator().RandiRange(0, itemsOnGround.Length - 1);
			var item = itemsOnGround[index];
			if (item == null) return;

			SaveManager.Instance.CurrentSave.AddItem(item.Id, 1);
			SaveManager.Instance.SaveToDisk();
			await MessageManager.PlayText(item.Sprite, new string[] { $"You found a {item.Name}!" });
		}
	}
	

}
