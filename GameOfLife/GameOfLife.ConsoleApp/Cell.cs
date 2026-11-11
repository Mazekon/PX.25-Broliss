namespace GameOfLife.ConsoleApp
{
    internal class Cell
    {
        public bool IsAlive { get; set; }
        public string AliveSymbol { get; set; } = "🟢";
        public string DeadSymbol { get; set; } = "⚪";

        public Cell(bool isAlive = false)
        {
            IsAlive = isAlive;
        }

        public string GetSymbol()
        {
            return IsAlive ? AliveSymbol : DeadSymbol;
        }
    }
}
