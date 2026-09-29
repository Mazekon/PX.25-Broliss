namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IMenuManager menuManager = new MenuManager();
            menuManager.Run();
        }
    }
}