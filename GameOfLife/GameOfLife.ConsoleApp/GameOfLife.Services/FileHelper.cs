using System.Text.Json;
using GameOfLife.Data;

namespace GameOfLife.Services
{
    public class FileHelper : IFileHelper
    {
        public string? LastSavedFileName { get; private set; }

        public bool TrySaveGame(GridDataobj gameData)
        {
            return gameData != null && TrySaveGames(new[] { gameData });
        }

        public bool TrySaveGames(IReadOnlyList<GridDataobj> games)
        {
            return TrySaveGames(games, Enumerable.Range(0, Math.Min(games?.Count ?? 0, 8)).ToList());
        }

        public bool TrySaveGames(IReadOnlyList<GridDataobj> games, IReadOnlyList<int> displayedGames, IReadOnlyList<int>? deadGenerations = null)
        {
            LastSavedFileName = null;

            if (games == null || games.Count == 0)
                return false;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"GameSave {DateTime.Now:yyyy-MM-dd HHmmss}.json";
                string filePath = Path.Combine(desktopPath, fileName);
                var session = new SavedGamesSession
                {
                    Games = games.Select((game, index) => new SavedGameData
                    {
                        CurrentGeneration = game.CurrentGeneration,
                        CurrentGrid = game.CurrentGrid,
                        DeadGenerations = deadGenerations != null && index < deadGenerations.Count ? deadGenerations[index] : 0
                    }).ToList(),
                    DisplayedGames = displayedGames.Where(index => index >= 0 && index < games.Count).Take(8).ToList()
                };

                string json = JsonSerializer.Serialize(session, CreateJsonOptions());
                File.WriteAllText(filePath, json);
                LastSavedFileName = fileName;
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
            return LoadGames(fileName)?.FirstOrDefault();
        }

        public List<GridDataobj>? LoadGames(string fileName)
        {
            SavedGamesSession? session = LoadSession(fileName);
            return session?.Games.Select(game => new GridDataobj(game.CurrentGrid)
            {
                CurrentGeneration = game.CurrentGeneration
            }).ToList();
        }

        public SavedGamesSession? LoadSession(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, fileName);
                string json = File.ReadAllText(filePath);
                using JsonDocument document = JsonDocument.Parse(json);

                if (document.RootElement.ValueKind == JsonValueKind.Array)
                {
                    List<SavedGameData>? oldGames = JsonSerializer.Deserialize<List<SavedGameData>>(json, CreateJsonOptions());
                    return oldGames == null ? null : new SavedGamesSession
                    {
                        Games = oldGames,
                        DisplayedGames = Enumerable.Range(0, Math.Min(oldGames.Count, 8)).ToList()
                    };
                }

                return JsonSerializer.Deserialize<SavedGamesSession>(json, CreateJsonOptions());
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
    }

    public class SavedGamesSession
    {
        public List<SavedGameData> Games { get; set; } = new();
        public List<int> DisplayedGames { get; set; } = new();
    }

    public class SavedGameData
    {
        public int CurrentGeneration { get; set; }
        public CellStatus[,] CurrentGrid { get; set; } = new CellStatus[0, 0];
        public int DeadGenerations { get; set; }
    }
}
