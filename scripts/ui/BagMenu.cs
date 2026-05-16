using Godot;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;
using Logger = Game.Core.Logger;

public partial class BagMenu : Node2D
{
	[ExportGroup("Slot UI Elements")]
	[Export] public RichTextLabel[] NameLabels;
	[Export] public RichTextLabel[] LevelLabels;
	[Export] public TextureProgressBar[] HpBars;
	[Export] public Sprite2D[] Sprites;
	[Export] public RichTextLabel Money;
	[Export] public VBoxContainer ItemsVBox;
	[Export] public VBoxContainer itemsContainer;
	[Export] public ItemList ItemList;

	private Dictionary<string, ItemResource> _itemCache = new();

	public override void _Ready()
	{
		BuildItemCache();

		var partyData = SaveManager.Instance?.CurrentSave?.PartyDetails;

		Money.Text = "¥" + SaveManager.Instance?.CurrentSave?.Money.ToString();
		

		if (partyData == null)
		{
			Logger.Error("Party Data is null!");
		}
		else
		{
			for (int i = 0; i < partyData.Count; i++)
			{
				if (i >= NameLabels.Length) break;

				var pokemonDict = partyData[i].AsGodotDictionary();
				Logger.Info(pokemonDict);

				var idKey = pokemonDict.ContainsKey("ID") ? "ID" : "Id";
				var pokemonID = (PokemonID)(int)pokemonDict[idKey];
				var pokemonResource = PokeBase.LoadPokemon(pokemonID);

				if (pokemonResource == null)
				{
					GD.PrintErr($"Could not load resource for ID: {pokemonID}");
					continue;
				}

				if (NameLabels[i] != null)
					NameLabels[i].Text = pokemonResource.Name;

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
		}

		RefreshItems();
	}

	private void BuildItemCache()
	{
		using var dir = DirAccess.Open("res://resources/items/all/");
		if (dir == null) return;

		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (fileName.EndsWith(".tres"))
			{
				var res = ResourceLoader.Load<ItemResource>($"res://resources/items/all/{fileName}");
				if (res != null)
					_itemCache[res.Id.ToString()] = res;
			}
			fileName = dir.GetNext();
		}
	}

	public void RefreshItems()
	{
		ItemList.Clear();

		var items = SaveManager.Instance?.CurrentSave?.Inventory;
		if (items == null || items.Count == 0) return;

		foreach (var entry in items)
		{
			string itemId = entry.Key.AsString();
			int quantity = entry.Value.AsInt32();

			_itemCache.TryGetValue(itemId, out ItemResource itemResource);

			string itemName = itemResource != null ? itemResource.Name : $"Item #{itemId}";
			
			int idx = ItemList.AddItem($"{itemName}  x{quantity}");
			
			if (itemResource?.Sprite != null)
				ItemList.SetItemIcon(idx, itemResource.Sprite);
		}
	}
}
