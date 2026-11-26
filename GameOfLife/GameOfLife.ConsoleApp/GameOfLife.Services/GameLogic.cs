namespace GameOfLife.Services
{
    public class GameLogic : IGameLogic
    {
        public Cell[,] CreateRandomGrid(int size)
        {
            Cell[,] grid = new Cell[size, size];

            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                    grid[row, column] = new Cell(Random.Shared.NextDouble() < 0.25);
            }

            return grid;
        }

        public Cell[,] GetNextGeneration(Cell[,] grid)
        {
            int rows = grid.GetLength(0);
            int columns = grid.GetLength(1);
            Cell[,] nextGrid = new Cell[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int neighbors = CountLiveNeighbors(grid, row, column);
                    bool alive = grid[row, column].IsAlive;
                    bool nextState = alive ? neighbors == 2 || neighbors == 3 : neighbors == 3;
                    nextGrid[row, column] = new Cell(nextState);
                }
            }

            return nextGrid;
        }

        public int CountLiveCells(Cell[,] grid)
        {
            int count = 0;

            foreach (Cell cell in grid)
            {
                if (cell.IsAlive)
                    count++;
            }

            return count;
        }

        private int CountLiveNeighbors(Cell[,] grid, int row, int column)
        {
            int count = 0;

            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    if (rowOffset == 0 && columnOffset == 0)
                        continue;

                    int neighborRow = row + rowOffset;
                    int neighborColumn = column + columnOffset;

                    if (neighborRow >= 0 && neighborRow < grid.GetLength(0) &&
                        neighborColumn >= 0 && neighborColumn < grid.GetLength(1) &&
                        grid[neighborRow, neighborColumn].IsAlive)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}

