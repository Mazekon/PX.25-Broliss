using System;
using GameOfLife.Data;
using GameOfLife.Services;

namespace GameOfLife.ConsoleApp
{
    internal class MenuManager : IMenuManager
    {
        private const int DefaultMinSize = 2;
        private const int DefaultMaxSize = 10;
        private const int MaxGames = 1000;
        private const int MaxDisplayedGames = 8;

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

            int size;
            while (true)
            {
                Console.Write($"Enter game field size ({DefaultMinSize}-{DefaultMaxSize}): ");
                if (int.TryParse(Console.ReadLine(), out size) && size >= DefaultMinSize && size <= DefaultMaxSize)
                    break;

                Console.WriteLine($"Size must be between {DefaultMinSize} and {DefaultMaxSize}.");
            }

            var games = new List<Cell[,]>();
            for (int index = 0; index < gameCount; index++)
                games.Add(GetDefaultGrid(size));

            PlayGames(games, Enumerable.Range(0, Math.Min(games.Count, MaxDisplayedGames)).ToList());
        }

        private void LoadGame()
        {
            string fileName = new FileMenu().ChooseFile();
            if (string.IsNullOrEmpty(fileName))
                return;

            SavedGamesSession? session = new FileHelper().LoadSession(fileName);
            if (session == null || session.Games.Count == 0)
            {
                Console.WriteLine("Games could not be loaded.");
                Console.ReadLine();
                return;
            }

            var games = session.Games.Select(game => ConvertToCells(game.CurrentGrid)).ToList();
            List<int> displayedGames = session.DisplayedGames
                .Where(index => index >= 0 && index < games.Count)
                .Take(MaxDisplayedGames)
                .ToList();

            if (displayedGames.Count == 0)
                displayedGames = Enumerable.Range(0, Math.Min(games.Count, MaxDisplayedGames)).ToList();

            PlayGames(games, displayedGames, session.Games[0].CurrentGeneration, session.Games.Select(game => game.DeadGenerations).ToArray());
        }

        private void PlayGames(List<Cell[,]> games, List<int> displayedGames, int generation = 0, int[]? savedDeadGenerations = null)
        {
            int[] liveCounts = games.Select(CountLiveCells).ToArray();
            int[] amplitudes = new int[games.Count];
            int[] deadGenerations = savedDeadGenerations != null && savedDeadGenerations.Length == games.Count
                ? savedDeadGenerations
                : new int[games.Count];
            bool[] deadGames = liveCounts.Select((count, index) => count == 0 || deadGenerations[index] > 0).ToArray();
            bool paused = false;
            string message = string.Empty;

            while (true)
            {
                PrintGames(games, displayedGames, generation, liveCounts, amplitudes, deadGames, deadGenerations, paused, message);
                DateTime nextGeneration = DateTime.UtcNow.AddSeconds(1);
                bool redraw = false;

                while (paused || DateTime.UtcNow < nextGeneration)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Escape)
                            return;

                        if (key == ConsoleKey.L)
                        {
                            LoadGame();
                            return;
                        }

                        if (key == ConsoleKey.P)
                        {
                            paused = !paused;
                            message = paused ? "Game paused." : "Game resumed.";
                            break;
                        }

                        if (key == ConsoleKey.V)
                        {
                            displayedGames = ChooseDisplayedGames(games, displayedGames, deadGames, liveCounts, generation);
                            message = "Displayed games updated.";
                            redraw = true;
                            break;
                        }

                        if (key == ConsoleKey.S)
                        {
                            var data = games.Select(game => CreateGameData(game, generation)).ToList();
                            var fileHelper = new FileHelper();
                            bool saved = fileHelper.TrySaveGames(data, displayedGames, deadGenerations);
                            message = saved ? $"Saved: {fileHelper.LastSavedFileName}" : "Games could not be saved.";
                        }
                    }
                    if (!paused)
                        System.Threading.Thread.Sleep(50);
                }

                if (paused || redraw)
                    continue;

                Parallel.ForEach(Enumerable.Range(0, games.Count), index =>
                {
                    if (deadGames[index])
                    {
                        deadGenerations[index]++;
                        return;
                    }

                    games[index] = GetNextGeneration(games[index]);
                    int currentLiveCount = CountLiveCells(games[index]);
                    amplitudes[index] = Math.Abs(currentLiveCount - liveCounts[index]);
                    liveCounts[index] = currentLiveCount;

                    if (currentLiveCount == 0)
                    {
                        deadGames[index] = true;
                        deadGenerations[index] = 1;
                    }
                });

                generation++;
                message = string.Empty;
            }
        }

        private void PrintGames(List<Cell[,]> games, List<int> displayedGames, int generation, int[] liveCounts, int[] amplitudes, bool[] deadGames, int[] deadGenerations, bool paused, string message)
        {
            ClearConsole();
            int gamesPerRow = Math.Max(1, Math.Min(4, Console.WindowWidth / GetGameBlockWidth(games[displayedGames[0]])));

            for (int first = 0; first < displayedGames.Count; first += gamesPerRow)
            {
                int last = Math.Min(first + gamesPerRow, displayedGames.Count);
                for (int index = first; index < last; index++)
                {
                    int gameIndex = displayedGames[index];
                    Console.Write($"Game {gameIndex + 1}".PadRight(GetGameBlockWidth(games[gameIndex])));
                }
                Console.WriteLine();

                int maxRows = displayedGames.Skip(first).Take(last - first).Max(gameIndex => games[gameIndex].GetLength(0));
                for (int row = 0; row < maxRows; row++)
                {
                    for (int index = first; index < last; index++)
                    {
                        Cell[,] game = games[displayedGames[index]];
                        string line = row < game.GetLength(0)
                            ? string.Concat(Enumerable.Range(0, game.GetLength(1)).Select(column => game[row, column].GetSymbol()))
                            : string.Empty;
                        Console.Write(line.PadRight(GetGameBlockWidth(game)));
                    }
                    Console.WriteLine();
                }

                for (int index = first; index < last; index++)
                    Console.Write($"Generation: {generation}".PadRight(GetGameBlockWidth(games[displayedGames[index]])));
                Console.WriteLine();

                for (int index = first; index < last; index++)
                {
                    int gameIndex = displayedGames[index];
                    Console.Write($"Live cells: {liveCounts[gameIndex]}".PadRight(GetGameBlockWidth(games[gameIndex])));
                }
                Console.WriteLine();

                for (int index = first; index < last; index++)
                {
                    int gameIndex = displayedGames[index];
                    Console.Write($"Amplitude: {amplitudes[gameIndex]}".PadRight(GetGameBlockWidth(games[gameIndex])));
                }
                Console.WriteLine();

                for (int index = first; index < last; index++)
                {
                    int gameIndex = displayedGames[index];
                    string status = deadGames[gameIndex] ? $"FINISHED ({deadGenerations[gameIndex]})" : "ACTIVE";
                    Console.Write(status.PadRight(GetGameBlockWidth(games[gameIndex])));
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine($"P: {(paused ? "resume" : "pause")} | V: choose games | S: save all games | L: load game | Esc: main menu");
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        private List<int> ChooseDisplayedGames(List<Cell[,]> games, List<int> currentSelection, bool[] deadGames, int[] liveCounts, int generation)
        {
            var selected = new HashSet<int>(currentSelection);
            int cursor = currentSelection.Count > 0 ? currentSelection[0] : 0;
            int firstVisible = 0;
            int visibleRows = Math.Max(5, Math.Min(20, Console.WindowHeight - 6));

            while (true)
            {
                ClearConsole();
                Console.WriteLine("Choose games to display:");
                Console.WriteLine("Up/Down: move | Space: select | Enter: confirm | Esc: cancel");
                Console.WriteLine($"Selected: {selected.Count}/{MaxDisplayedGames}");
                Console.WriteLine();

                if (cursor < firstVisible)
                    firstVisible = cursor;
                if (cursor >= firstVisible + visibleRows)
                    firstVisible = cursor - visibleRows + 1;

                int lastVisible = Math.Min(firstVisible + visibleRows, games.Count);
                for (int index = firstVisible; index < lastVisible; index++)
                {
                    bool focused = index == cursor;
                    bool isSelected = selected.Contains(index);
                    string status = deadGames[index] ? "FINISHED" : "ACTIVE";
                    string line = $"{(isSelected ? "[X]" : "[ ]")} Game {index + 1}, Generation: {generation}, Live cells: {liveCounts[index]}, {status}";

                    if (focused)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = deadGames[index] ? ConsoleColor.Red : ConsoleColor.Green;
                    }

                    Console.WriteLine($"{(focused ? ">" : " ")} {line}");
                    Console.ResetColor();
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Escape)
                    return currentSelection;

                if (key == ConsoleKey.Enter)
                    return selected.Count == 0 ? currentSelection : selected.OrderBy(index => index).ToList();

                if (key == ConsoleKey.UpArrow)
                    cursor = cursor == 0 ? games.Count - 1 : cursor - 1;

                if (key == ConsoleKey.DownArrow)
                    cursor = cursor == games.Count - 1 ? 0 : cursor + 1;

                if (key == ConsoleKey.Spacebar)
                {
                    if (selected.Contains(cursor))
                        selected.Remove(cursor);
                    else
                    {
                        if (selected.Count == MaxDisplayedGames)
                            selected.Remove(selected.First());

                        selected.Add(cursor);
                    }
                }
            }
        }

        private void ClearConsole()
        {
            try
            {
                Console.Write("\u001b[2J\u001b[3J\u001b[H");
                Console.Out.Flush();
            }
            catch (IOException)
            {
                Console.Clear();
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.Clear();
            }
        }

        private int GetGameBlockWidth(Cell[,] game)
        {
            return game.GetLength(1) * 4;
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
                    bool alive = grid[row, column].IsAlive;
                    nextGrid[row, column] = new Cell(alive ? neighbors == 2 || neighbors == 3 : neighbors == 3);
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
                    if (neighborRow >= 0 && neighborRow < grid.GetLength(0) && neighborColumn >= 0 && neighborColumn < grid.GetLength(1) && grid[neighborRow, neighborColumn].IsAlive)
                        count++;
                }
            }
            return count;
        }

        private static int CountLiveCells(Cell[,] grid)
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
                    Console.Write(grid[row, column].GetSymbol());
                Console.WriteLine();
            }
        }

    }
}
