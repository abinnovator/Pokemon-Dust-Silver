using Godot;
using System;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;

public partial class BagMenu : Node2D
{
	[ExportGroup("Slot UI Elements")]
	[Export] public RichTextLabel[] NameLabels;
	[Export] public RichTextLabel[] LevelLabels;
	[Export] public TextureProgressBar[] HpBars;
	[Export] public Sprite2D[] Sprites;
	[Export] public ScrollContainer ItemsContainer;
	[Export] public RichTextLabel Money;
	[Export] public VBoxContainer ItemsVBox;

	public override void _Ready()
	{
		var partyData = SaveManager.Instance?.CurrentSave?.PartyDetails;
		var items = SaveManager.Instance?.CurrentSave?.Inventory;

		Money.Text = "¥" + SaveManager.Instance?.CurrentSave?.Money.ToString();

		if (partyData == null)
		{
			Game.Core.Logger.Error("Party Data is null!");
		}
		else
		{
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

		if (items == null || ItemsVBox == null) return;

		foreach (var entry in items)
		{
			string itemId = entry.Key.AsString();
			int quantity = entry.Value.AsInt32();

			ItemResource itemResource = null;
			using var dir = DirAccess.Open("res://resources/items/all/");
			if (dir != null)
			{
				dir.ListDirBegin();
				string fileName = dir.GetNext();
				while (fileName != "")
				{
					if (fileName.EndsWith(".tres"))
					{
						var res = ResourceLoader.Load<ItemResource>($"res://resources/items/all/{fileName}");
						if (res != null && res.Id.ToString() == itemId)
						{
							itemResource = res;
							break;
						}
					}
					fileName = dir.GetNext();
				}
			}

			var label = new RichTextLabel();
			label.FitContent = true;
			label.CustomMinimumSize = new Vector2(0, 64);
			label.Text = itemResource != null ? $"{itemResource.Name} x{quantity}" : $"Item #{itemId} x{quantity}";

			ItemsVBox.AddChild(label);
		}
	}
}
