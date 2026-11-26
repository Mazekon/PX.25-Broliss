namespace GameOfLife.Services
{
    public interface IGameLogic
    {
        Cell[,] CreateRandomGrid(int size);
        Cell[,] GetNextGeneration(Cell[,] grid);
        int CountLiveCells(Cell[,] grid);
    }
}

