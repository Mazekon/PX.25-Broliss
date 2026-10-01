namespace GameOfLife.ConsoleApp
{
    internal interface IMenuManager
    {
        void Run();
        void ShowMenu();
        void StartGame();
        void PrintGrid(Cell[,] grid);
    }
}
