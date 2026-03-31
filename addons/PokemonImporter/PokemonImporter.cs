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
    public const string apiPath = "https://pokeapi.co/api/v2/pokemon/";
    public const string evolutionChainApiPath = "https://pokeapi.co/api/v2/evolution-chain/";
    public const string menuIconApiPath = "https://img.pokemondb.net/sprites/lets-go-pikachu-eevee/normal/";

    public override void _EnterTree()
    {
        AddToolMenuItem(importMenuItemText, Callable.From(OnImportPokemonClicked));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem(importMenuItemText);
    }

    private void OnImportPokemonClicked()
    {
        ImportPokemon();
    }

    private async void ImportPokemon()
    {
        Logger.Info("Attempting to import pokemon.");
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(spriteFolderPath));

        const int gcInterval = 10;
        for (int i = 1; i <= Globals.POKEMON_NUMBERS; i++)
        {
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

            // Fetch species data
            var speciesUrl = pokemonData.Species?.Url;
            PokemonSpeciesResponse speciesData = null;
            if (!string.IsNullOrEmpty(speciesUrl))
            {
                speciesData = await Modules.FetchDataFromPokeApi<PokemonSpeciesResponse>(speciesUrl);
            }

            // Fetch evolution chain data
            EvolutionChainResponse evolutionData = null;
            var evolutionChainUrl = speciesData?.EvolutionChain?.Url;
            if (!string.IsNullOrEmpty(evolutionChainUrl))
            {
                evolutionData = await Modules.FetchDataFromPokeApi<EvolutionChainResponse>(evolutionChainUrl);
            }

            Logger.Info($"Creating resource for pokemon {pokemonName}...");
            await CreatePokemonResource(pokemonName, pokemonData, speciesData, evolutionData);

            if (i % gcInterval == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Logger.Info($"Collected garbage after pokemon {i}");
            }
            await Task.Delay(100);
        }
    }

    private async Task CreatePokemonResource(
        string pokemonName,
        PokemonApiResponse pokemonData,
        PokemonSpeciesResponse speciesData,
        EvolutionChainResponse evolutionData)
    {
        var flavorTextEntries = speciesData?.FlavorTextEntries;
        var description = flavorTextEntries?.FirstOrDefault(entry => entry.Language.Name == "en");

        var pokemon = new PokemonResource()
        {
            Name = pokemonName,
            Id = pokemonData.Id,
            Height = pokemonData.Height,
            Weight = pokemonData.Weight,
            BaseExperience = pokemonData.BaseExperience,
            Description = description?.FlavorText ?? "",
        };

        // Map Types
        if (pokemonData.Types != null && pokemonData.Types.Count > 0)
        {
            pokemon.TypeOne = PokemonEnum.TypeMap.TryGetValue(
                pokemonData.Types[0].Type?.Name ?? "", out var type1) ? type1 : PokemonType.None;
            if (pokemonData.Types.Count > 1)
            {
                pokemon.TypeTwo = PokemonEnum.TypeMap.TryGetValue(
                    pokemonData.Types[1].Type?.Name ?? "", out var type2) ? type2 : PokemonType.None;
            }
        }

        // Map Stats
        var stats = pokemonData.Stats;
        for (int i = 0; i < stats.Count; i++)
        {
            var stat = stats[i].Stat;
            var value = stats[i].BaseStat;
            var parsed = PokemonEnum.StatMap.TryGetValue(stat.Name, out var parsedStat) ? parsedStat : PokemonStat.None;
            switch (parsed)
            {
                case PokemonStat.Hp: pokemon.BaseHp = value; break;
                case PokemonStat.Attack: pokemon.BaseAttack = value; break;
                case PokemonStat.Defense: pokemon.BaseDefense = value; break;
                case PokemonStat.SpecialAttack: pokemon.BaseSpecialAttack = value; break;
                case PokemonStat.SpecialDefense: pokemon.BaseSpecialDefense = value; break;
                case PokemonStat.Speed: pokemon.BaseSpeed = value; break;
            }
        }

        // Map Moves
        var moves = pokemonData.Moves;
        pokemon.LearnableMoves = GetLearnableMoves(moves);
        pokemon.LevelUpMoves = GetLevelUpMoves(moves);

        // Map Evolution
        if (evolutionData?.Chain != null)
        {
            var evoInfo = FindEvolution(evolutionData.Chain, pokemonName);
            if (evoInfo != null)
            {
                pokemon.CanEvolve = true;
                if (Enum.TryParse<PokemonID>(evoInfo.Value.EvolvesTo.Replace("-", "_"), true, out var evoId))
                    pokemon.EvolvesInto = evoId;
                pokemon.EvolutionLevel = evoInfo.Value.Level;
            }
        }

        // Map Sprites
        var sprites = pokemonData.Sprites;
        pokemon.FrontSprite = await LoadTextureFromUrl(
            sprites.front_default, spriteFolderPath, $"{pokemonName}_front.png");
        pokemon.ShinyFrontSprite = await LoadTextureFromUrl(
            sprites.front_shiny, spriteFolderPath, $"{pokemonName}_shiny_front.png");
        pokemon.BackSprite = await LoadTextureFromUrl(
            sprites.back_default, spriteFolderPath, $"{pokemonName}_back.png");
        pokemon.ShinyBackSprite = await LoadTextureFromUrl(
            sprites.back_shiny, spriteFolderPath, $"{pokemonName}_shiny_back.png");
        pokemon.MenuIconSprite = await LoadTextureFromUrl(
            $"{menuIconApiPath}{pokemonName}.png", spriteFolderPath, $"{pokemonName}_menu_icon.png");

        // Save resource
        var savePath = $"{folderPath}{pokemonName.ToLower()}.tres";
        var result = ResourceSaver.Save(pokemon, savePath);

        if (result != Error.Ok)
            Logger.Error($"Failed to save pokemon resource for {pokemonName}: {result}");
        else
            Logger.Info($"Successfully saved pokemon resource for {pokemonName} to {savePath}");
    }

    private (string EvolvesTo, int Level)? FindEvolution(ChainLink chain, string pokemonName)
    {
        if (chain.Species?.Name == pokemonName && chain.EvolvesTo?.Count > 0)
        {
            var next = chain.EvolvesTo[0];
            var detail = next.EvolutionDetails?.FirstOrDefault();
            int level = detail?.MinLevel ?? 0;
            return (next.Species.Name, level);
        }

        if (chain.EvolvesTo != null)
        {
            foreach (var link in chain.EvolvesTo)
            {
                var result = FindEvolution(link, pokemonName);
                if (result != null) return result;
            }
        }

        return null;
    }

    private Godot.Collections.Array<string> GetLearnableMoves(List<PokemonMoveEntry> moves)
    {
        Godot.Collections.Array<string> learnableMoves = new();

        if (moves == null) return learnableMoves;

        foreach (var moveEntry in moves)
        {
            if (moveEntry.Move != null && !string.IsNullOrEmpty(moveEntry.Move.Name))
                learnableMoves.Add(moveEntry.Move.Name);
        }

        return learnableMoves;
    }

    private Godot.Collections.Dictionary<string, int> GetLevelUpMoves(List<PokemonMoveEntry> moves)
    {
        Godot.Collections.Dictionary<string, int> levelUpMoves = new();

        if (moves == null) return levelUpMoves;

        foreach (var moveEntry in moves)
        {
            var levelDetail = moveEntry.VersionGroupDetails.FirstOrDefault(
                d => d.MoveLearnMethod.Name == "level-up");

            if (levelDetail != null)
            {
                string moveName = moveEntry.Move.Name;
                int level = levelDetail.LevelLearnedAt;
                if (!levelUpMoves.ContainsKey(moveName))
                    levelUpMoves.Add(moveName, level);
            }
        }

        return levelUpMoves;
    }

    private async Task<Texture2D> LoadTextureFromUrl(string imageUrl, string folder, string fileName)
    {
        string resourcePath = $"{folder}{fileName}";
        string fullSavePath = ProjectSettings.GlobalizePath(resourcePath);

        try
        {
            if (!File.Exists(fullSavePath))
            {
                string downloadedTexture = await Modules.DownloadSprite(imageUrl, folder, fileName);
                if (downloadedTexture == null) return null;
            }

            byte[] imageBytes = File.ReadAllBytes(fullSavePath);
            var image = new Image();
            var error = image.LoadPngFromBuffer(imageBytes);
            if (error != Error.Ok)
            {
                Logger.Error($"Failed to load texture from url: {error}");
                return null;
            }

            var texture = ImageTexture.CreateFromImage(image);
            return texture;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load texture from url: {e.Message}");
            return null;
        }
    }
}
#endif