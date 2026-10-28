using System;

namespace GameOfLife.Data
{
    public class CellStatus
    {
        public bool Current { get; set; }
        public bool Previous { get; set; }

        public CellStatus(bool current = false)
        {
            Current = current;
            Previous = false;
        }

        public void Commit()
        {
            Previous = Current;
        }
    }
}
