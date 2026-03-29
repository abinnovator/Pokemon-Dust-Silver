#if TOOLS
using Game.Core;
using Game.Gameplay;
using Godot;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Logger = Game.Core.Logger;
using System.Collections.Generic;

[Tool]
public partial class item_importer : EditorPlugin
{
	private const string ImportMenuItemText = "Import All Items";
	private const string FolderPath = "res://resources/items/all/";
	private const string SpriteFolderPath = "res://assets/items/all/";
	private const string ApiPath = "https://pokeapi.co/api/v2/item/";

	private static readonly HashSet<string> PokeballSlugs = new()
	{
		"poke-ball", "great-ball", "ultra-ball", "master-ball", "safari-ball",
		"net-ball", "dive-ball", "nest-ball", "repeat-ball", "timer-ball",
		"luxury-ball", "premier-ball", "dusk-ball", "heal-ball", "quick-ball",
		"cherish-ball", "dream-ball", "beast-ball", "lure-ball", "level-ball",
		"moon-ball", "heavy-ball", "fast-ball", "friend-ball", "love-ball",
		"park-ball", "sport-ball", "strange-ball", "left-poke-ball",
		"lastrange-ball", "lapoke-ball", "lagreat-ball", "laultra-ball",
		"laheavy-ball", "laleaden-ball", "lagigaton-ball", "lafeather-ball",
		"lawing-ball", "lajet-ball", "laorigin-ball"
	};

	public override void _EnterTree()
	{
		AddToolMenuItem(ImportMenuItemText, Callable.From(OnImportAllItemsClicked));
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(ImportMenuItemText);
	}

	private void OnImportAllItemsClicked()
	{
		ImportAllItems();
	}

	private async void ImportAllItems()
	{
		Logger.Info("Fetching item list...");
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(FolderPath));
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(SpriteFolderPath));

		// Fetch the full item list from PokeAPI
		var listData = await Modules.FetchDataFromPokeApi<ItemListResponse>($"{ApiPath}?limit=9999");
		if (listData == null)
		{
			Logger.Error("Failed to fetch item list!");
			return;
		}

		var itemsToImport = listData.Results
			.Where(i => !PokeballSlugs.Contains(i.Name))
			.ToList();

		Logger.Info($"Found {itemsToImport.Count} items to import (excluding pokeballs).");

		int success = 0;
		int failed = 0;

		for (int i = 0; i < itemsToImport.Count; i++)
		{
			var item = itemsToImport[i];
			Logger.Info($"[{i + 1}/{itemsToImport.Count}] Importing {item.Name}...");

			var data = await Modules.FetchDataFromPokeApi<ItemApiResponse>($"{ApiPath}{item.Name}");
			if (data == null)
			{
				Logger.Warning($"Failed to fetch data for {item.Name}, skipping.");
				failed++;
				continue;
			}

			await CreateItemResource(item.Name, data);
			success++;
			await Task.Delay(150); // be polite to the API
		}

		Logger.Info($"Item import complete! {success} succeeded, {failed} failed.");
	}

	private async Task CreateItemResource(string slug, ItemApiResponse data)
	{
		var resource = new ItemResource
		{
			Name = data.Name,
			Id = data.Id,
			Cost = data.Cost,
			Category = data.Category?.Name ?? "",
			Attributes = data.Attributes?.Select(a => a.Name).ToArray() ?? Array.Empty<string>(),
		};

		// English description
		var englishFlavor = data.FlavorTextEntries?.FirstOrDefault(e => e.Language?.Name == "en");
		if (englishFlavor != null)
			resource.Description = englishFlavor.FlavorText?.Replace("\n", " ").Replace("\f", " ") ?? "";

		// English effect
		var englishEffect = data.EffectEntries?.FirstOrDefault(e => e.Language?.Name == "en");
		if (englishEffect != null)
		{
			resource.ShortEffect = englishEffect.ShortEffect ?? "";
			resource.Effect = englishEffect.Effect?.Replace("\n", " ").Replace("\f", " ") ?? "";
		}

		// Fling power
		resource.FlingPower = data.FlingPower ?? 0;

		// Sprite
		if (data.Sprites?.Default != null)
		{
			string fileName = $"{slug}.png";
			resource.Sprite = await LoadTextureFromUrl(data.Sprites.Default, SpriteFolderPath, fileName);
		}

		string savePath = $"{FolderPath}{slug}.tres";
		var result = ResourceSaver.Save(resource, savePath);
		if (result != Error.Ok)
			Logger.Error($"Failed to save ItemResource for {slug}: {result}");
		else
			Logger.Info($"Saved {slug} to {savePath}");
	}

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

	// ---- API response models ----

	private class ItemListResponse
	{
		[JsonPropertyName("results")]
		public ItemEntry[] Results { get; set; }
	}

	private class ItemEntry
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("url")]
		public string Url { get; set; }
	}
}
#endif
