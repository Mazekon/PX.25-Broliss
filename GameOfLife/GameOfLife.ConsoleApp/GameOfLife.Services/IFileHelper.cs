using GameOfLife.Data;

namespace GameOfLife.Services
{
    public interface IFileHelper
    {
        bool TrySaveGame(GridDataobj gameData);
        bool TrySaveGames(IReadOnlyList<GridDataobj> games);
        bool TrySaveGames(IReadOnlyList<GridDataobj> games, IReadOnlyList<int> displayedGames, IReadOnlyList<int>? deadGenerations = null);
        GridDataobj? LoadGame(string fileName);
        List<GridDataobj>? LoadGames(string fileName);
        SavedGamesSession? LoadSession(string fileName);
    }
}
