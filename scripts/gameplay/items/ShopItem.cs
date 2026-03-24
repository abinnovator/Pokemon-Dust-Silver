using Godot;

namespace Game.Gameplay;

public enum ItemCategory
{
	Pokeball,
	Medicine,
	BattleItem,
	Berry,
	KeyItem,
	TM,
	Mail,
	General
}

[GlobalClass]
[Tool]
public partial class ShopItem : Resource
{
	[ExportCategory("Basic Info")]
	[Export]
	public string ItemName = "Item";
	
	[Export]
	public int ItemId;
	
	[Export(PropertyHint.MultilineText)]
	public string Description = "A useful item.";
	
	[Export]
	public int Price = 100;

	[ExportCategory("Item Properties")]
	[Export]
	public ItemCategory Category = ItemCategory.General;
	
	[Export]
	public int Stock = -1; // -1 means infinite stock
	
	[Export]
	public bool IsKeyItem = false;

	[ExportCategory("Visuals")]
	[Export]
	public Texture2D Icon;
}
