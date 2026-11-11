using System;
using GameOfLife.Data;
using GameOfLife.Services;

namespace GameOfLife.ConsoleApp
{
    internal class MenuManager : IMenuManager
    {
        private const int MinSize = 2;
        private const int MaxSize = 10;
        private const int MaxGames = 8;

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
            Console.Write($"Enter number of games (1-{MaxGames}): ");

            if (!int.TryParse(Console.ReadLine(), out int gameCount) || gameCount < 1 || gameCount > MaxGames)
                return;

            Console.Write($"Enter game field size ({MinSize}-{MaxSize}): ");
            if (!int.TryParse(Console.ReadLine(), out int size) || size < MinSize || size > MaxSize)
                return;

            var games = new List<Cell[,]>();
            for (int i = 0; i < gameCount; i++)
                games.Add(GetDefaultGrid(size));

            PlayGames(games);
        }

        private void LoadGame()
        {
            var fileMenu = new FileMenu();
            string fileName = fileMenu.ChooseFile();

            if (string.IsNullOrEmpty(fileName))
                return;

            var fileHelper = new FileHelper();
            List<GridDataobj>? savedGames = fileHelper.LoadGames(fileName);

            if (savedGames == null || savedGames.Count == 0)
            {
                Console.WriteLine("Games could not be loaded.");
                Console.ReadLine();
                return;
            }

            var games = savedGames.Take(MaxGames).Select(game => ConvertToCells(game.CurrentGrid)).ToList();
            int generation = savedGames[0].CurrentGeneration;
            PlayGames(games, generation);
        }

        private void PlayGames(List<Cell[,]> games, int generation = 0)
        {
            int[] liveCounts = games.Select(CountLiveCells).ToArray();
            int[] amplitudes = new int[games.Count];
            string message = string.Empty;

            while (true)
            {
                PrintGames(games, generation, liveCounts, amplitudes, message);
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
                            var gameData = games.Select((game, index) => CreateGameData(game, generation)).ToList();
                            bool saved = fileHelper.TrySaveGames(gameData);
                            message = saved ? "Games saved." : "Games could not be saved.";
                        }
                    }

                    System.Threading.Thread.Sleep(50);
                }

                Parallel.ForEach(Enumerable.Range(0, games.Count), index =>
                {
                    games[index] = GetNextGeneration(games[index]);
                    int currentLiveCount = CountLiveCells(games[index]);
                    amplitudes[index] = Math.Abs(currentLiveCount - liveCounts[index]);
                    liveCounts[index] = currentLiveCount;
                });

                generation++;
                message = string.Empty;
            }
        }

        private void PrintGames(List<Cell[,]> games, int generation, int[] liveCounts, int[] amplitudes, string message)
        {
            Console.Clear();

            const int gamesPerRow = 4;

            for (int firstGame = 0; firstGame < games.Count; firstGame += gamesPerRow)
            {
                int lastGame = Math.Min(firstGame + gamesPerRow, games.Count);

                for (int gameIndex = firstGame; gameIndex < lastGame; gameIndex++)
                    Console.Write($"Game {gameIndex + 1}".PadRight(GetGameBlockWidth(games[gameIndex])));
                Console.WriteLine();

                int maxRows = 0;
                for (int gameIndex = firstGame; gameIndex < lastGame; gameIndex++)
                    maxRows = Math.Max(maxRows, games[gameIndex].GetLength(0));

                for (int row = 0; row < maxRows; row++)
                {
                    for (int gameIndex = firstGame; gameIndex < lastGame; gameIndex++)
                    {
                        Cell[,] game = games[gameIndex];
                        string line = row < game.GetLength(0)
                            ? string.Concat(Enumerable.Range(0, game.GetLength(1)).Select(column => game[row, column].GetSymbol()))
                            : string.Empty;
                        Console.Write(line.PadRight(GetGameBlockWidth(game)));
                    }

                    Console.WriteLine();
                }

                for (int gameIndex = firstGame; gameIndex < lastGame; gameIndex++)
                    Console.Write($"Generation: {generation}".PadRight(GetGameBlockWidth(games[gameIndex])));
                Console.WriteLine();

                for (int gameIndex = firstGame; gameIndex < lastGame; gameIndex++)
                    Console.Write($"Live cells: {liveCounts[gameIndex]}".PadRight(GetGameBlockWidth(games[gameIndex])));
                Console.WriteLine();

                for (int gameIndex = firstGame; gameIndex < lastGame; gameIndex++)
                    Console.Write($"Amplitude: {amplitudes[gameIndex]}".PadRight(GetGameBlockWidth(games[gameIndex])));
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("S: save all games | Esc: exit");

            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        private int GetGameBlockWidth(Cell[,] game)
        {
            return game.GetLength(1) * 4 + 2;
        }

        private GridDataobj CreateGameData(Cell[,] grid, int generation)
        {
            var currentGrid = new CellStatus[grid.GetLength(0), grid.GetLength(1)];

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                    currentGrid[row, column] = new CellStatus(grid[row, column].IsAlive);
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
                    grid[row, column] = new Cell(statusGrid[row, column].Current);
            }

            return grid;
        }

        private Cell[,] GetDefaultGrid(int size)
        {
            Cell[,] grid = new Cell[size, size];

            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                    grid[row, column] = new Cell(Random.Shared.NextDouble() < 0.25);
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
                    bool nextState = isAlive ? neighbors == 2 || neighbors == 3 : neighbors == 3;
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

        private int CountLiveCells(Cell[,] grid)
        {
            int count = 0;

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                {
                    if (grid[row, column].IsAlive)
                        count++;
                }
            }

            return count;
        }

        public void PrintGrid(Cell[,] grid)
        {
            Console.Clear();

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int column = 0; column < grid.GetLength(1); column++)
                    Console.Write(grid[row, column].GetSymbol() + " ");

                Console.WriteLine();
            }
        }
    }
}
