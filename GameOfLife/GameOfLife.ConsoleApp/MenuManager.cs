using System;

namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
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

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("1) Start new game");
            Console.WriteLine("2) Exit");
            Console.Write("\nSelect an option: ");
        }

        private static void StartGame()
        {
            Console.Clear();
            Console.Write("Enter game field size (2-50): ");

            if (!int.TryParse(Console.ReadLine(), out int size) || size < 2 || size > 50)
            {
                Console.ReadLine();
                return;
            }

            int[,] grid = GetDefaultGrid(size);
            PrintGrid(grid);

            Console.ReadLine();
        }

        private static int[,] GetDefaultGrid(int size)
        {
            int[,] grid = new int[size, size];
            return grid;
        }

        private static void PrintGrid(int[,] grid)
        {
            Console.Clear();

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(grid[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
