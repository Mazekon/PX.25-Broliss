
using System;

namespace GameOfLife.Data
{
    public class GridDataobj
    {
        public int CurrentGeneration { get; set; }

        public CellStatus[,] CurrentGrid { get; set; }

        public GridDataobj(CellStatus[,] grid = null)
        {
            CurrentGrid = grid ?? new CellStatus[0, 0];
            CurrentGeneration = 0;
        }
    }
}

