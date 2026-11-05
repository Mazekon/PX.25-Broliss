using GameOfLife.Data;

namespace GameOfLife.Services
{
    public interface IFileHelper
    {
        bool TrySaveGame(GridDataobj gameData);
        bool TrySaveGames(IReadOnlyList<GridDataobj> games);
        GridDataobj? LoadGame(string fileName);
        List<GridDataobj>? LoadGames(string fileName);
    }
}
