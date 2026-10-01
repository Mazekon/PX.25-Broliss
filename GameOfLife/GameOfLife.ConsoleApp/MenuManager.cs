using System;

namespace GameOfLife.ConsoleApp
{
    internal class MenuManager : IMenuManager
    {
        private const int MinSize = 2;
        private const int MaxSize = 50;
        /// <summary>
        /// will attempt to return grid, if it exists.
        /// otherwise returns a new
        /// </summary>

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
            Console.WriteLine("2) Exit");
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

            Cell[,] grid = GetDefaultGrid(size);
            PrintGrid(grid);

            Console.ReadLine();
        }

        private Cell[,] GetDefaultGrid(int size)
        {
            Cell[,] grid = new Cell[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    grid[i, j] = new Cell(false);
                }
            }
            return grid;
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
