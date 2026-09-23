internal class Program
{
    private static int Main(string[] args)
    {

        var showMenu = true;

        while (showMenu)
        {
            Console.Clear();

            Console.WriteLine("choose an option:");
            Console.WriteLine("1) Start Game");
            Console.WriteLine("2) Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    StartGame();
                    showMenu = true;
                    break;
                case "2":
                    showMenu = false;
                    break;
                default:
                    showMenu = true;
                    break;
            }
        }

        return 0;
    }
    private static void StartGame()
    {
        Console.Clear();
        Console.WriteLine("You started the game!");
        Thread.Sleep(1500);
    }
}