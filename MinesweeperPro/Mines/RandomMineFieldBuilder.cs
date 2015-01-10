using System.Collections.Generic;

namespace MinesweeperPro.Mines
{
    public class RandomMineFieldBuilder : IMineFieldBuilder
    {
        public void BuildMineField(MineField mineField)
        {
            Field = new CellState[Desc.Rows, Desc.Columns];

            var freeCells = new List<Coord>();

            for (int i = 0; i < Desc.Rows; ++i)
            {
                for (int j = 0; j < Desc.Columns; ++j)
                {
                    var coord = new Coord(i, j);
                    Field[i, j] = new CellState(mineField, coord);
                    freeCells.Add(coord);
                }
            }

            for (int i = 0; i < Desc.Mines; ++i)
            {
                var freeCellIdx = Randomness.RNG.Next(freeCells.Count);
                var mineAt = freeCells[freeCellIdx];
                freeCells.RemoveAt(freeCellIdx);

                Field.GetCellAt(mineAt).HasMine = true;
                foreach (var adj in MineField.Adjacencies)
                {
                    var adjPos = adj + mineAt;
                    var adjCell = Field.GetCellAt(adjPos);
                    if (adjCell != null)
                        adjCell.AdjacentMines++;
                }
            }
        }

        public CellState[,] Field { get; private set; }
        public MineFieldDesc Desc { get; set; }
    }
}
