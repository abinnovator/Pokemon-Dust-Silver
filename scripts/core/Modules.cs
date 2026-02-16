using Godot;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Game.Core
{
    public static class Modules
    {
        public static bool IsActionJustPressed()
        {
            return Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down") || Input.IsActionJustPressed("ui_left") || Input.IsActionJustPressed("ui_right");
        }
        public static bool IsActionPressed()
        {
            return Input.IsActionPressed("ui_up") || Input.IsActionPressed("ui_down") || Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right");
        }
        public static bool IsActionJustReleased()
        {
            return Input.IsActionJustReleased("ui_up") || Input.IsActionJustReleased("ui_down") || Input.IsActionJustReleased("ui_left") || Input.IsActionJustReleased("ui_right");
        }
        public static Vector2I ConvertVector2ToVector2I (Vector2 vector)
        {
            return new Vector2I ((int)vector.X / Globals.GridSize, (int)vector.Y / Globals.GridSize);
        }
        public static Vector2 ConvertVector2IToVector2 (Vector2I vector)
        {
            return new Vector2 (vector.X* Globals.GridSize, vector.Y* Globals.GridSize);
        }

        private static readonly System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
        public static async Task<T> FetchDataFromPokeApi<T>(string url){
			try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.Error($"Failed to fetch data from {url}: {response.StatusCode}");
                    return default;
                }
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch(System.Exception ex)
            {
                Logger.Error($"Failed to fetch data from {url}: {ex.Message}");
                return default;
            }
		}
        public static async Task<string> DownloadSprite(string imageUrl, string saveFolderPath, string fileName)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;
            string fullSavePath = ProjectSettings.GlobalizePath($"{saveFolderPath}{fileName}");
            string resourcePath = $"{saveFolderPath}{fileName}";
            try
            {
                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                File.WriteAllBytes(fullSavePath, imageBytes);
                return resourcePath;
            }
            catch (System.Exception e)
            {
                Logger.Error($"Failed to download sprite from {imageUrl} to {resourcePath}: {e.Message}");
                return null;
            }
        }
    }
}