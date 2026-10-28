using GameOfLife.Data;

namespace GameOfLife.Services
{
    public interface IFileHelper
    {
        bool TrySaveGame(GridDataobj gameData);
    }
}
