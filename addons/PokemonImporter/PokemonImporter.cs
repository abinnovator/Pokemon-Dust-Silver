#if TOOLS
using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Array = System.Array;
using Logger = Game.Core.Logger;


[Tool]
public partial class PokemonImporter : EditorPlugin
{
	private const string importMenuItemText = "Import Pokemon";
	private const string folderPath = "res://resources/pokemon/";
	private const string spriteFolderPath = "res://assets/pokemon/";
	public const string apiPath= "https://pokeapi.co/api/v2/pokemon/";
	public const string menuIconApiPath = "https://img.pokemondb.net/sprites/lets-go-pikachu-eevee/normal/";
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddToolMenuItem(importMenuItemText, Callable.From(ImportPokemon));
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveToolMenuItem(importMenuItemText);
	}
	public async void ImportPokemon(){
		Logger.Info("Attempting to import pokemon.");
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(spriteFolderPath));

		const int gcInterval = 10;
		for (int i=1 ; i<=Globals.POKEMON_NUMBERS ; i++){
			PokemonApiResponse pokemonData = await Modules.FetchDataFromPokeApi<PokemonApiResponse>($"{apiPath}{i}");
			
			if (pokemonData == null)
			{
				Logger.Warning($"Failed to fetch data for pokemon {i}");
				continue;
			}

			var pokemonName = pokemonData.Name;
			
			if (string.IsNullOrEmpty(pokemonName))
			{
				Logger.Warning($"Pokemon {i} has no name");
				continue;
			}
			
			var speciesUrl = pokemonData.Species?.Url;
			PokemonSpeciesResponse speciesData = null;
			if (!string.IsNullOrEmpty(speciesUrl))
			{
				speciesData = await Modules.FetchDataFromPokeApi<PokemonSpeciesResponse>(speciesUrl);
			}

			Logger.Info($"Creating resource for pokemon {pokemonName}...");
			await CreatePokemonResource(pokemonName, pokemonData, speciesData);

			if (i % gcInterval == 0){
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Logger.Info($"Collected garbage after pokemon {i}");
			}
			await Task.Delay(100);
			
		}
	} 
	private async Task CreatePokemonResource(string pokemonName , PokemonApiResponse pokemonData, PokemonSpeciesResponse speciesData)
	{	
		var FlavorTextEntries = speciesData?.FlavorTextEntries;
		var description = FlavorTextEntries.FirstOrDefault(entry => entry.Language.Name == "en");
		var pokemon = new PokemonResource(){
			Name = pokemonName,
			Id = pokemonData.Id,
			Height = pokemonData.Height,
			Weight = pokemonData.Weight,
			BaseExperience = pokemonData.BaseExperience,
			Description = description.FlavorText,
		};
		// var pokemon = new PokemonResource()
		// {
		// 	Name = pokemonName,
		// 	Id = pokemonData.Id,
		// 	Height = pokemonData.Height,
		// 	Weight = pokemonData.Weight,
		// 	BaseExperience = pokemonData.BaseExperience,
		// };

		// // Map Types
		// if (pokemonData.Types != null && pokemonData.Types.Count > 0)
		// {
		// 	pokemon.TypeOne = PokemonEnum.TypeMap.TryGetValue(pokemonData.Types[0].Type?.Name ?? "", out var type1) ? type1 : PokemonType.None;
		// 	if (pokemonData.Types.Count > 1)
		// 	{
		// 		pokemon.TypeTwo = PokemonEnum.TypeMap.TryGetValue(pokemonData.Types[1].Type?.Name ?? "", out var type2) ? type2 : PokemonType.None;
		// 	}
		// }

		// // Map Stats
		// if (pokemonData.Stats != null)
		// {
		// 	foreach (var statSlot in pokemonData.Stats)
		// 	{
		// 		var statName = statSlot.Stat?.Name;
		// 		var value = statSlot.BaseStat;
		// 		switch (statName)
		// 		{
		// 			case "hp": pokemon.BaseHp = value; break;
		// 			case "attack": pokemon.BaseAttack = value; break;
		// 			case "defense": pokemon.BaseDefense = value; break;
		// 			case "special-attack": pokemon.BaseSpecialAttack = value; break;
		// 			case "special-defense": pokemon.BaseSpecialDefense = value; break;
		// 			case "speed": pokemon.BaseSpeed = value; break;
		// 		}
		// 	}
		// }

		// // Map Description from Flavor Text
		// if (speciesData?.FlavorTextEntries != null)
		// {
		// 	var englishEntry = speciesData.FlavorTextEntries.Find(e => e.Language?.Name == "en");
		// 	if (englishEntry != null)
		// 	{
		// 		pokemon.Description = englishEntry.FlavorText.Replace("\n", " ").Replace("\f", " ");
		// 	}
		// }

		// var savePath = $"{folderPath}{pokemonName.ToLower()}.tres";
		// var result = ResourceSaver.Save(pokemon, savePath);
		// if (result != Error.Ok)
		// {
		// 	Logger.Error($"Failed to save pokemon resource for {pokemonName}: {result}");
		// }
		// else
		// {
		// 	Logger.Info($"Successfully saved pokemon resource for {pokemonName} to {savePath}");
		// }
		Logger.Info($"Successfully saved pokemon resource for {pokemonName}");

		var stats = pokemonData.Stats;

		for (int i=0 ; i<stats.Count ; i++){
			var stat = stats[i].Stat;
			var value = stats[i].BaseStat;
			var parsed = PokemonEnum.StatMap.TryGetValue(stat.Name, out var parsedStat) ? parsedStat : PokemonStat.None;
			switch (parsed){
				case PokemonStat.Hp: pokemon.BaseHp = value; break;
				case PokemonStat.Attack: pokemon.BaseAttack = value; break;
				case PokemonStat.Defense: pokemon.BaseDefense = value; break;
				case PokemonStat.SpecialAttack: pokemon.BaseSpecialAttack = value; break;
				case PokemonStat.SpecialDefense: pokemon.BaseSpecialDefense = value; break;
				case PokemonStat.Speed: pokemon.BaseSpeed = value; break;
			}
		}

		var moves = pokemonData.Moves;
		pokemon.LearnableMoves = GetLearnableMoves(moves);
		pokemon.LevelUpMoves = GetLevelUpMoves(moves);
		var sprites = pokemonData.Sprites;

		pokemon.FrontSprite = await LoadTextureFromUrl(sprites.front_default, spriteFolderPath, $"{pokemonName}_front.png");
		pokemon.ShinyFrontSprite = await LoadTextureFromUrl(sprites.front_shiny, spriteFolderPath, $"{pokemonName}_shiny_front.png");

		pokemon.BackSprite = await LoadTextureFromUrl(sprites.back_default, spriteFolderPath, $"{pokemonName}_back.png");
		pokemon.ShinyBackSprite = await LoadTextureFromUrl(sprites.back_shiny, spriteFolderPath, $"{pokemonName}_shiny_back.png");

		pokemon.MenuIconSprite = await LoadTextureFromUrl($"{menuIconApiPath}{pokemonName}.png", spriteFolderPath, $"{pokemonName}_menu_icon.png");

		var savePath = $"{folderPath}{pokemonName.ToLower()}.tres";
		var result = ResourceSaver.Save(pokemon, savePath);

		

		if (result != Error.Ok){
			Logger.Error($"Failed to save pokemon resource for {pokemonName}: {result}");
		}
		else{
			Logger.Info($"Successfully saved pokemon resource for {pokemonName} to {savePath}");
		}
	}
	// Change 'System.Array' to 'List<PokemonMoveEntry>'
	private Godot.Collections.Array<string> GetLearnableMoves(List<PokemonMoveEntry> moves)
	{
		Godot.Collections.Array<string> learnableMoves = new();
		
		if (moves == null)
		{
			return learnableMoves;
		}

		foreach (var moveEntry in moves)
		{
			// Accessing move.name from the JSON structure you provided
			if (moveEntry.Move != null && !string.IsNullOrEmpty(moveEntry.Move.Name))
			{
				learnableMoves.Add(moveEntry.Move.Name);
			}
		}
		
		return learnableMoves;
	}
	private Godot.Collections.Dictionary<string, int> GetLevelUpMoves(List<PokemonMoveEntry> moves)
	{
		// Fix the return type to match your Resource
		Godot.Collections.Dictionary<string, int> levelUpMoves = new();

		if (moves == null) return levelUpMoves;

		foreach (var moveEntry in moves)
		{
			// We only want moves learned by "level-up"
			// PokeAPI stores multiple ways to learn a move; we'll grab the first level-up occurrence
			var levelDetail = moveEntry.VersionGroupDetails.FirstOrDefault(d => d.MoveLearnMethod.Name == "level-up");

			if (levelDetail != null)
			{
				string moveName = moveEntry.Move.Name;
				int level = levelDetail.LevelLearnedAt;

				// Add to dictionary (check if it exists in case of duplicates across versions)
				if (!levelUpMoves.ContainsKey(moveName))
				{
					levelUpMoves.Add(moveName, level);
				}
			}
		}

		return levelUpMoves;
	}
	private async Task<Texture2D> LoadTextureFromUrl(string imageUrl, string folder, string fileName)
	{
		string resourcePath = $"{folder}{fileName}";
		string fullSavePath = ProjectSettings.GlobalizePath(resourcePath);
		
		try{
			if (!File.Exists(fullSavePath)){
				string downloadedTexture = await Modules.DownloadSprite(imageUrl, folder, fileName);
				if (downloadedTexture == null){
					return null;
				}
			}
			byte[] imageBytes = File.ReadAllBytes(fullSavePath);
			var image = new Image();
			var error = image.LoadPngFromBuffer(imageBytes);
			if (error != Error.Ok){
				Logger.Error($"Failed to load texture from url: {error}");
				return null;
			}
			var texture = ImageTexture.CreateFromImage(image);
			return texture;

		}
		catch(Exception e){
			Logger.Error($"Failed to load texture from url: {e.Message}");
			return null;
		}
	}
}
#endif
