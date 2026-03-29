using Godot;
using System;
using Game.Gameplay;


namespace Game.Gameplay
{
	public partial class ItemLabel : Node2D
	{
		[ExportCategory("ItemLabelNodes")]
		[Export] public Sprite2D Selected_bg;
		[Export] public RichTextLabel ItemName;
		[Export] public Sprite2D ItemIcon;
		[Export] public RichTextLabel ItemQuantity;
		[Export] public Boolean isSelected;
		[ExportCategory("ItemLabelInputs")]
		[Export] public ItemResource Item;
		private const string spriteFolderPath = "res://assets/items/all";
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			ItemName.Text = Item.Name;
			ItemIcon.Texture = Item.Sprite;
			Selected_bg.Visible = isSelected;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}
	}
