using System.Text.Json;
using GameOfLife.Data;

namespace GameOfLife.Services
{
    public class FileHelper : IFileHelper
    {
        public bool TrySaveGame(GridDataobj gameData)
        {
            if (gameData == null)
                return false;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"GameSave {DateTime.Now:yyyy-MM-dd HHmmss}.json";
                string filePath = Path.Combine(desktopPath, fileName);
                var savedGame = new SavedGameData
                {
                    CurrentGeneration = gameData.CurrentGeneration,
                    CurrentGrid = gameData.CurrentGrid
                };

                string json = JsonSerializer.Serialize(savedGame, CreateJsonOptions());
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public GridDataobj? LoadGame(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, fileName);
                string json = File.ReadAllText(filePath);
                SavedGameData? savedGame = JsonSerializer.Deserialize<SavedGameData>(json, CreateJsonOptions());

                if (savedGame == null || savedGame.CurrentGrid == null)
                    return null;

                return new GridDataobj(savedGame.CurrentGrid)
                {
                    CurrentGeneration = savedGame.CurrentGeneration
                };
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new JsonConverter());
            return options;
        }

        private class SavedGameData
        {
            public int CurrentGeneration { get; set; }
            public CellStatus[,] CurrentGrid { get; set; } = new CellStatus[0, 0];
        }
    }
}
