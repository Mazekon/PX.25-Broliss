namespace GameOfLife.ConsoleApp
{
    internal class Cell
    {
        public bool IsAlive { get; set; }
        public char AliveSymbol { get; set; } = 'X';
        public char DeadSymbol { get; set; } = '0';

        public Cell(bool isAlive = false)
        {
            IsAlive = isAlive;
        }

        public char GetSymbol()
        {
            return IsAlive ? AliveSymbol : DeadSymbol;
        }
    }
}
