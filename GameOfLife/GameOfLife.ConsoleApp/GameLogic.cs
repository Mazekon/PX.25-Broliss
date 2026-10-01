namespace GameOfLife.ConsopeApp
{

 public class GameLogic
    {
        private readonly char liveSymbol;
        private readonly char deadSymbol;

        public GameLogic(char liveSymbol = 'O', char deadSymbol = '.')
        {
            this.liveSymbol = liveSymbol;
            this.deadSymbol = deadSymbol;
        }

        public void InitializeBoard(char[,] board, double fillProbability = 0.25)
        {
            Random random = new Random();
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    board[r, c] = random.NextDouble() < fillProbability ? liveSymbol : deadSymbol;
                }
            }
        }
    }
}

public char[,] GetNextGeneration(char[,] currentBoard)