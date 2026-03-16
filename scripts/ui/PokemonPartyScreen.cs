using Godot;
using System;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay;

public partial class PokemonPartyScreen : Node2D
{
	[ExportGroup("Slot UI Elements")]
	[Export] public RichTextLabel[] NameLabels;
	[Export] public RichTextLabel[] LevelLabels;
	[Export] public TextureProgressBar[] HpBars;
	[Export] public Sprite2D[] Sprites;

	public override void _Ready()
	{
		var partyData = SaveManager.Instance?.CurrentSave?.PartyDetails;
		
		if (partyData == null) 
		{
			Game.Core.Logger.Error("Party Data is null!");
			return;
		}

		for (int i = 0; i < partyData.Count; i++)
		{
			if (i >= NameLabels.Length) break;

			// 1. Get the dictionary for this specific Pokemon
			var pokemonDict = partyData[i].AsGodotDictionary();
			Game.Core.Logger.Info(pokemonDict);

			// 2. Get the Resource based on the ID stored in the dictionary
			// Using "Id" or "ID" - make sure this matches your SaveManager casing!
			var idKey = pokemonDict.ContainsKey("ID") ? "ID" : "Id";
			// Convert to int first, then to the Enum
			var pokemonID = (PokemonID)(int)pokemonDict[idKey];
			var pokemonResource = PokeBase.LoadPokemon(pokemonID);

			if (pokemonResource == null)
			{
				GD.PrintErr($"Could not load resource for ID: {pokemonID}");
				continue;
			}

			// 3. Assign the Species Name from the Resource
			if (NameLabels[i] != null)
				NameLabels[i].Text = pokemonResource.Name;

			// 4. Assign the Level from the Save Data
			if (LevelLabels[i] != null && pokemonDict.ContainsKey("Level"))
				LevelLabels[i].Text =  pokemonDict["Level"].ToString();

			// 5. Assign HP from the Save Data
			if (HpBars[i] != null && pokemonDict.ContainsKey("CurrentHP"))
			{
				HpBars[i].MaxValue = pokemonResource.BaseHp;
				HpBars[i].Value = pokemonDict["CurrentHP"].AsInt32();
			}
			
			// 6. Assign Sprite from the Resource
			if (Sprites[i] != null)
				Sprites[i].Texture = pokemonResource.FrontSprite; 
		}
	}
}
