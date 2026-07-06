using Godot;
using Game.Core;
using Game.Gameplay;
using System.Threading.Tasks;

namespace Game.Gameplay
{
	public partial class ItemSpawnPoint : Area2D
{
	[Export] public ItemResource Item;
	[Export] public bool Collected = false;

	private Sprite2D _sprite;

	public override void _Ready()
		{
			if (Item == null || Collected) return;

			_sprite = new Sprite2D();
			// _sprite.Texture = Item.Sprite;
			AddChild(_sprite);

			// Add collision shape
			var shape = new CollisionShape2D();
			var rect = new RectangleShape2D();
			rect.Size = new Vector2(Globals.GridSize, Globals.GridSize);
			shape.Shape = rect;
			AddChild(shape);

			BodyEntered += OnBodyEntered;
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body is Player)
				_ = OnCollect();
		}

		public async Task OnCollect()
		{
			if (Collected || Item == null) return;

			var level = SceneManager.GetCurrentLevel();
			if (level == null) return;

			int chance = Globals.GetRandomNumberGenerator().RandiRange(0, 100);
			if (chance > level.itemEncounterRate) return;

			Collected = true;

			if (_sprite != null)
				_sprite.Visible = false;

			SaveManager.Instance.CurrentSave.AddItem(Item.Id, 1);
			SaveManager.Instance.SaveToDisk();
			await MessageManager.PlayText(Item.Sprite, new string[] { $"You found a {Item.Name}!" });
		}
	}
}
