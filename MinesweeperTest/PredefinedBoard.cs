using System.Collections.Generic;
using MinesweeperPro.Mines;
using System.Linq;

namespace MinesweeperTest
{
    public class PredefinedBoard : IMineFieldBuilder
    {
        public const char CoveredEmpty = '.';
        public const char Mine = '*';
        public const char FlaggedMine = '!';
        public const char FlaggedWrong = 'x';
        public const char EmptyNoAdjacent = ' ';

        private List<string> myRows = new List<string>();

        public PredefinedBoard Row(string row)
        {
            myRows.Add(row);
            return this;
        }

        public void Grow()
        {
            var rows = myRows.Count;
            for (int i = 0; i < rows; ++i)
            {
                myRows[i] = '.' + myRows[i] + '.';
            }

            var cols = myRows[0].Length;
            var emptyRow = new string('.', cols);
            myRows.Insert(0, emptyRow);
            myRows.Add(emptyRow);
        }

        public void BuildMineField(MineField mineField)
        {
            var mines = myRows.SelectMany(row => row).Count(c => c == Mine || c == FlaggedMine);

            Desc = new MineFieldDesc(myRows.Count, myRows[0].Length, mines);

            Field = new CellState[Desc.Rows, Desc.Columns];

            for (int i = 0; i < Desc.Rows; ++i)
            {
                for (int j = 0; j < Desc.Columns; ++j)
                {
                    var state = myRows[i][j];
                    var coord = new Coord(i, j);
                    var cell = new CellState(mineField, coord);
                    Field[i, j] = cell;

                    switch (state)
                    {
                        case CoveredEmpty:
                            break;

                        case Mine:
                            cell.HasMine = true;
                            break;

                        case FlaggedMine:
                            cell.HasMine = true;
                            cell.InterfaceState = CellInterfaceState.Flagged;
                            break;

                        case FlaggedWrong:
                            cell.InterfaceState = CellInterfaceState.Flagged;
                            break;

                        case EmptyNoAdjacent:
                            cell.InterfaceState = CellInterfaceState.Opened;
                            break;

                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                            cell.AdjacentMines = state - '0';
                            cell.InterfaceState = CellInterfaceState.Opened;
                            break;
                    }
                }
            }

            for (int i = 0; i < Desc.Rows; ++i)
            {
                for (int j = 0; j < Desc.Columns; ++j)
                {
                    var coord = new Coord(i, j);
                    if (!Field.GetCellAt(coord).HasMine)
                        continue;

                    foreach (var adj in MineField.Adjacencies)
                    {
                        var adjPos = adj + coord;
                        var adjCell = Field.GetCellAt(adjPos);
                        if (adjCell != null && !adjCell.IsOpen)
                            adjCell.AdjacentMines++;
                    }
                }
            }
        }

        public CellState[,] Field { get; private set; }
        public MineFieldDesc Desc { get; private set; }
    }
}
