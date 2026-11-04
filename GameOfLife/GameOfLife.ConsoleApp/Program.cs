using System.Text;

namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            IMenuManager menuManager = new MenuManager();
            menuManager.Run();
        }
    }
}