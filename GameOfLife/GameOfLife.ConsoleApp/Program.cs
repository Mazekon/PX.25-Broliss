using System.Security.AccessControl;

internal class Program
{
    private static int Main(string[] args)
    {
        var showMenu = true;
        while (showMenu)
        {
            Console.Clear();

            Console.WriteLine("Choose an option: ");
            Console.WriteLine("1) Start Game");
            Console.WriteLine("2) Options");
            Console.WriteLine("3) Exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    StartGame();
                    showMenu = true;
                    break;
                case "2":
                    FieldCreating();
                    showMenu = true;
                    break;
                case "3":
                    showMenu = false;
                    break;
                default:
                    showMenu = false;
                    break;
            }
        }
        return 0;
    }

    private static int[,] FieldCreating()
    {
        Console.Clear();
        while (true)
        {
            Console.Clear();
            Console.Write("Enter game field size: ");
            if (!int.TryParse(Console.ReadLine(), out int size))
            {
                Console.WriteLine("Enter a number!\n");
                continue;
            }
            if (size <= 1)
            {
                Console.WriteLine("Too small size!\n");
                continue;
            }
            if (size > 100)
            {
                Console.WriteLine("Too big size!\n");
                continue;
            }
            int[,] matrix = new int[size, size];
            return matrix;
        }
    }

    private static void StartGame()
    {
        Console.Clear();
        Console.WriteLine("You started a game!");
        Thread.Sleep(1500);
    }

}