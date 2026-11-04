namespace GameOfLife.ConsoleApp
{
    public interface IFileMenu
    {
        string ChooseFile();
    }

    public class FileMenu : IFileMenu
    {
        public string ChooseFile()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string[] files = Directory.GetFiles(desktopPath, "GameSave *.json");

            if (files.Length == 0)
                return string.Empty;

            int selectedIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select a saved game:");

                for (int i = 0; i < files.Length; i++)
                {
                    string marker = i == selectedIndex ? ">" : " ";
                    Console.WriteLine($"{marker} {Path.GetFileName(files[i])}");
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Enter)
                    return Path.GetFileName(files[selectedIndex]);

                if (key == ConsoleKey.Escape)
                    return string.Empty;

                if (key == ConsoleKey.UpArrow)
                    selectedIndex = (selectedIndex - 1 + files.Length) % files.Length;

                if (key == ConsoleKey.DownArrow)
                    selectedIndex = (selectedIndex + 1) % files.Length;
            }
        }
    }
}
