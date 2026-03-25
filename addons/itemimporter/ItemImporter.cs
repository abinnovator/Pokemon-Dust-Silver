#if TOOLS
using Game.Core;
using Game.Gameplay;
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Logger = Game.Core.Logger;

[Tool]
public partial class ItemImporter : EditorPlugin
{
	private const string ImportMenuItemText = "Import Pokeballs";
	private const string FolderPath = "res://resources/items/pokeballs/";
	private const string SpriteFolderPath = "res://assets/items/pokeballs/";
	private const string ApiPath = "https://pokeapi.co/api/v2/item/";

	// Maps our internal Pokeball enum names to their PokeAPI item slugs
	private static readonly (Game.Core.Pokeball Type, string ApiSlug)[] PokeballMap =
	{
		(Game.Core.Pokeball.Normal,  "poke-ball"),
		(Game.Core.Pokeball.Master,  "master-ball"),
		(Game.Core.Pokeball.Great,   "great-ball"),
		(Game.Core.Pokeball.Safari,  "safari-ball"),
		(Game.Core.Pokeball.Level,   "level-ball"),
		(Game.Core.Pokeball.Lure,    "lure-ball"),
		(Game.Core.Pokeball.Moon,    "moon-ball"),
		(Game.Core.Pokeball.Friend,  "friend-ball"),
		(Game.Core.Pokeball.Love,    "love-ball"),
		(Game.Core.Pokeball.Fast,    "fast-ball"),
		(Game.Core.Pokeball.Sport,   "sport-ball"),
		(Game.Core.Pokeball.Premier, "premier-ball"),
		(Game.Core.Pokeball.Net,     "net-ball"),
		(Game.Core.Pokeball.Dive,    "dive-ball"),
		(Game.Core.Pokeball.Ultra,   "ultra-ball"),
		(Game.Core.Pokeball.Repeat,  "repeat-ball"),
		(Game.Core.Pokeball.Timer,   "timer-ball"),
		(Game.Core.Pokeball.Nest,    "nest-ball"),
		(Game.Core.Pokeball.Heal,    "heal-ball"),
		(Game.Core.Pokeball.Quick,   "quick-ball"),
		(Game.Core.Pokeball.Dusk,    "dusk-ball"),
		(Game.Core.Pokeball.Luxury,  "luxury-ball"),
		(Game.Core.Pokeball.Beast,   "beast-ball"),
		(Game.Core.Pokeball.Heavy,   "heavy-ball"),
		(Game.Core.Pokeball.Origin,  "origin-ball"),
	};

	public override void _EnterTree()
	{
		AddToolMenuItem(ImportMenuItemText, Callable.From(OnImportPokeballsClicked));
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(ImportMenuItemText);
	}

	private void OnImportPokeballsClicked()
	{
		ImportPokeballs();
	}

	private async void ImportPokeballs()
	{
		Logger.Info("Starting Pokeball import...");
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(FolderPath));
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(SpriteFolderPath));

		foreach (var (type, slug) in PokeballMap)
		{
			Logger.Info($"Importing {type} ({slug})...");
			var data = await Modules.FetchDataFromPokeApi<ItemApiResponse>($"{ApiPath}{slug}");
			if (data == null)
			{
				Logger.Warning($"Failed to fetch item data for {slug}");
				continue;
			}

			await CreatePokeballResource(type, data);
			await Task.Delay(150); // be polite to the API
		}

		Logger.Info("Pokeball import complete!");
	}

	private async Task CreatePokeballResource(Game.Core.Pokeball type, ItemApiResponse data)
	{
		var resource = new PokeballResource
		{
			Name = data.Name,
			Id = data.Id,
			Cost = data.Cost,
			// PokeAPI doesn't expose catch rate directly on item endpoint;
			// we set sensible defaults here that you can tune in the Inspector.
			CatchRate = GetDefaultCatchRate(type),
		};

		// English description
		var englishEntry = data.FlavorTextEntries?.FirstOrDefault(e => e.Language?.Name == "en");
		if (englishEntry != null)
			resource.Description = englishEntry.FlavorText?.Replace("\n", " ").Replace("\f", " ") ?? "";

		// Sprite
		if (data.Sprites?.Default != null)
		{
			string fileName = $"{type.ToString().ToLower()}.png";
			resource.Sprite = await LoadTextureFromUrl(data.Sprites.Default, SpriteFolderPath, fileName);
		}

		string savePath = $"{FolderPath}{type.ToString().ToLower()}.tres";
		var result = ResourceSaver.Save(resource, savePath);
		if (result != Error.Ok)
			Logger.Error($"Failed to save PokeballResource for {type}: {result}");
		else
			Logger.Info($"Saved {type} to {savePath}");
	}

	/// <summary>
	/// Returns a sensible default catch rate multiplier for each ball.
	/// These match the main-series game mechanics roughly.
	/// </summary>
	private static float GetDefaultCatchRate(Game.Core.Pokeball type) => type switch
	{
		Game.Core.Pokeball.Master  => 255f,  // always catches
		Game.Core.Pokeball.Ultra   => 2.0f,
		Game.Core.Pokeball.Great   => 1.5f,
		Game.Core.Pokeball.Normal  => 1.0f,
		Game.Core.Pokeball.Safari  => 1.5f,
		Game.Core.Pokeball.Net     => 3.0f,  // bug/water bonus handled elsewhere
		Game.Core.Pokeball.Dive    => 3.5f,
		Game.Core.Pokeball.Nest    => 1.0f,  // scales with low HP, base 1x
		Game.Core.Pokeball.Repeat  => 3.0f,
		Game.Core.Pokeball.Timer   => 1.0f,  // scales with turns
		Game.Core.Pokeball.Quick   => 5.0f,  // first turn bonus
		Game.Core.Pokeball.Dusk    => 3.5f,
		Game.Core.Pokeball.Heal    => 1.0f,
		Game.Core.Pokeball.Luxury  => 1.0f,
		Game.Core.Pokeball.Premier => 1.0f,
		Game.Core.Pokeball.Beast   => 5.0f,  // vs. Ultra Beasts
		Game.Core.Pokeball.Heavy   => 1.0f,  // weight-based, base 1x
		Game.Core.Pokeball.Love    => 8.0f,  // opposite gender bonus
		Game.Core.Pokeball.Moon    => 4.0f,
		Game.Core.Pokeball.Level   => 1.0f,
		Game.Core.Pokeball.Lure    => 3.0f,
		Game.Core.Pokeball.Friend  => 1.0f,
		Game.Core.Pokeball.Fast    => 4.0f,
		Game.Core.Pokeball.Sport   => 1.5f,
		Game.Core.Pokeball.Origin  => 1.0f,
		_ => 1.0f
	};

	private async Task<Texture2D> LoadTextureFromUrl(string imageUrl, string folder, string fileName)
	{
		string fullSavePath = ProjectSettings.GlobalizePath($"{folder}{fileName}");
		try
		{
			if (!File.Exists(fullSavePath))
			{
				string downloaded = await Modules.DownloadSprite(imageUrl, folder, fileName);
				if (downloaded == null) return null;
			}
			byte[] bytes = File.ReadAllBytes(fullSavePath);
			var image = new Image();
			if (image.LoadPngFromBuffer(bytes) != Error.Ok) return null;
			return ImageTexture.CreateFromImage(image);
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to load texture: {e.Message}");
			return null;
		}
	}
}
#endif
