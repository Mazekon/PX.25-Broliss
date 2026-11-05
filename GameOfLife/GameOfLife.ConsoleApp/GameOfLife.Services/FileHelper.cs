using System.Text.Json;
using GameOfLife.Data;

namespace GameOfLife.Services
{
    public class FileHelper : IFileHelper
    {
        public bool TrySaveGame(GridDataobj gameData)
        {
            return gameData != null && TrySaveGames(new[] { gameData });
        }

        public bool TrySaveGames(IReadOnlyList<GridDataobj> games)
        {
            if (games == null || games.Count == 0)
                return false;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"GameSave {DateTime.Now:yyyy-MM-dd HHmmss}.json";
                string filePath = Path.Combine(desktopPath, fileName);
                var savedGames = games.Select(game => new SavedGameData
                {
                    CurrentGeneration = game.CurrentGeneration,
                    CurrentGrid = game.CurrentGrid
                }).ToList();

                string json = JsonSerializer.Serialize(savedGames, CreateJsonOptions());
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
            catch (JsonException)
            {
                return false;
            }
        }

        public GridDataobj? LoadGame(string fileName)
        {
            List<GridDataobj>? games = LoadGames(fileName);
            return games?.FirstOrDefault();
        }

        public List<GridDataobj>? LoadGames(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, fileName);
                string json = File.ReadAllText(filePath);
                List<SavedGameData>? savedGames = JsonSerializer.Deserialize<List<SavedGameData>>(json, CreateJsonOptions());

                if (savedGames == null || savedGames.Count == 0)
                    return null;

                return savedGames.Select(savedGame => new GridDataobj(savedGame.CurrentGrid)
                {
                    CurrentGeneration = savedGame.CurrentGeneration
                }).ToList();
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
