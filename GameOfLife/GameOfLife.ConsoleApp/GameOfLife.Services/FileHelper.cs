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

                CellStatus[][] serializableGrid = null;
                if (gameData.CurrentGrid != null)
                {
                    int rows = gameData.CurrentGrid.GetLength(0);
                    int columns = gameData.CurrentGrid.GetLength(1);
                    serializableGrid = new CellStatus[rows][];

                    for (int row = 0; row < rows; row++)
                    {
                        serializableGrid[row] = new CellStatus[columns];
                        for (int column = 0; column < columns; column++)
                        {
                            serializableGrid[row][column] = gameData.CurrentGrid[row, column];
                        }
                    }
                }

                var serializableGameData = new
                {
                    gameData.CurrentGeneration,
                    CurrentGrid = serializableGrid
                };

                string json = JsonSerializer.Serialize(serializableGameData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

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
    }
}
