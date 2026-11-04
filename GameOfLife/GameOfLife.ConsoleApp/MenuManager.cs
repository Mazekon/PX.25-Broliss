using System;
using GameOfLife.Data;
using GameOfLife.Services;

namespace GameOfLife.ConsoleApp
{
    internal class MenuManager : IMenuManager
    {
        private const int MinSize = 2;
        private const int MaxSize = 50;


        public void Run()
        {
            var running = true;
            while (running)
            {
                ShowMenu();
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        StartGame();
                        break;
                    case "2":
                        LoadGame();
                        break;
                    case "3":
                        running = false;
                        break;
                    default:
                        Console.ReadLine();
                        break;
                }
            }
        }

        public void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("1) Start new game");
            Console.WriteLine("2) Load game");
            Console.WriteLine("3) Exit");
            Console.Write("\nSelect an option: ");
        }

        public void StartGame()
        {
            Console.Clear();
            Console.Write($"Enter game field size ({MinSize}-{MaxSize}): ");

            if (!int.TryParse(Console.ReadLine(), out int size) || size < MinSize || size > MaxSize)
            {
                Console.ReadLine();
                return;
            }

            PlayGame(GetDefaultGrid(size));
        }

        private void LoadGame()
        {
            var fileMenu = new FileMenu();
            string fileName = fileMenu.ChooseFile();

            if (string.IsNullOrEmpty(fileName))
                return;

            var fileHelper = new FileHelper();
            GridDataobj? gameData = fileHelper.LoadGame(fileName);

            if (gameData == null)
            {
                Console.WriteLine("Game could not be loaded.");
                Console.ReadLine();
                return;
            }

            PlayGame(ConvertToCells(gameData.CurrentGrid), gameData.CurrentGeneration);
        }

        private void PlayGame(Cell[,] grid, int generation = 0)
        {
            string message = string.Empty;

            while (true)
            {
                PrintGrid(grid);
                Console.WriteLine($"Generation: {generation}");
                Console.WriteLine("S: save | Esc: exit");
                if (!string.IsNullOrEmpty(message))
                    Console.WriteLine(message);

                DateTime nextGeneration = DateTime.UtcNow.AddSeconds(1);
                while (DateTime.UtcNow < nextGeneration)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey key = Console.ReadKey(true).Key;

                        if (key == ConsoleKey.Escape)
                            return;

                        if (key == ConsoleKey.S)
                        {
                            var fileHelper = new FileHelper();
                            bool saved = fileHelper.TrySaveGame(CreateGameData(grid, generation));
                            message = saved ? "Game saved." : "Game could not be saved.";
                        }
                    }

                    System.Threading.Thread.Sleep(50);
                }

                grid = GetNextGeneration(grid);
                generation++;
                message = string.Empty;
            }
        }

        private GridDataobj CreateGameData(Cell[,] grid, int generation)
        {
            var currentGrid = new CellStatus[grid.GetLength(0), grid.GetLength(1)];

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                {
                    currentGrid[row, column] = new CellStatus(grid[row, column].IsAlive);
                }
            }

            return new GridDataobj(currentGrid)
            {
                CurrentGeneration = generation
            };
        }

        private Cell[,] ConvertToCells(CellStatus[,] statusGrid)
        {
            Cell[,] grid = new Cell[statusGrid.GetLength(0), statusGrid.GetLength(1)];

            for (int row = 0; row < statusGrid.GetLength(0); row++)
            {
                for (int column = 0; column < statusGrid.GetLength(1); column++)
                {
                    grid[row, column] = new Cell(statusGrid[row, column].Current);
                }
            }

            return grid;
        }

        private Cell[,] GetDefaultGrid(int size)
        {
            Cell[,] grid = new Cell[size, size];
            Random random = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    grid[i, j] = new Cell(random.NextDouble() < 0.25);
                }
            }
            return grid;
        }

        private Cell[,] GetNextGeneration(Cell[,] grid)
        {
            int rows = grid.GetLength(0);
            int columns = grid.GetLength(1);
            Cell[,] nextGrid = new Cell[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int neighbors = CountLiveNeighbors(grid, row, column);
                    bool isAlive = grid[row, column].IsAlive;
                    bool nextState = isAlive
                        ? neighbors == 2 || neighbors == 3
                        : neighbors == 3;

                    nextGrid[row, column] = new Cell(nextState);
                }
            }

            return nextGrid;
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

        public void PrintGrid(Cell[,] grid)
        {
            Console.Clear();

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(grid[i, j].GetSymbol() + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
