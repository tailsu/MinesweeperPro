using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesweeperPro.Mines
{
    public delegate void AdjacentCellAction(CellState adjCell, Coord adjCoord);

    public class GameOverEventArgs : EventArgs
    {
        public bool Detonated { get; set; }
    }
    
    public class MineField : Bindable
    {
        public static readonly Coord[] Adjacencies = new[]
            {
                new Coord(-1, -1), new Coord(0, -1), new Coord(1, -1),
                new Coord(-1, 0), /*new Coord(0, 0),*/ new Coord(1, 0),
                new Coord(-1, 1), new Coord(0, 1), new Coord(1, 1),
            };

        private CellState[,] myField;
        private int? myRemainingFlags;
        private int myOpenedCells;
        private bool myGameOver;

        public MineFieldDesc FieldDesc { get; private set; }

        public bool IsGameOver
        {
            get { return myGameOver; }
        }

        public event EventHandler<GameOverEventArgs> GameOver;

        public int? RemainingFlags
        {
            get { return myRemainingFlags; }
            internal set { SetValue(ref myRemainingFlags, value); }
        }

        public void RecreateField(IMineFieldBuilder builder)
        {
            builder.BuildMineField(this);
            FieldDesc = builder.Desc;
            myField = builder.Field;

            this.RemainingFlags = FieldDesc.Mines - AllCells.Count(c => c.IsFlagged);
            myOpenedCells = AllCells.Count(c => c.IsOpen);
            myGameOver = false;
        }

        public CellState[,] Field { get { return myField; } }

        public CellState GetCellAt(Coord coord)
        {
            return myField.GetCellAt(coord);
        }

        public CellState GetCellAt(int row, int column)
        {
            return GetCellAt(new Coord(row, column));
        }

        public void Open(Coord coord)
        {
            var cell = GetCellAt(coord);
            if (cell == null)
                return;

            if (cell.InterfaceState == CellInterfaceState.Opened)
                return;

            cell.InterfaceState = CellInterfaceState.Opened;
            myOpenedCells++;

            if (cell.HasMine)
            {
                SignalGameOver(true);
                return;
            }

            if (cell.AdjacentMines == 0)
            {
                ForEachAdjacency(coord, (adjCell, adjCoord) => Open(adjCoord));
            }

            if (myOpenedCells == FieldDesc.Rows * FieldDesc.Columns - FieldDesc.Mines)
                SignalGameOver(false);
        }

        private void SignalGameOver(bool detonated)
        {
            if (GameOver != null && !myGameOver)
            {
                myGameOver = true;
                GameOver(this, new GameOverEventArgs { Detonated = detonated });
            }
        }

        public void ForEachAdjacency(Coord center, AdjacentCellAction action)
        {
            foreach (var adj in Adjacencies)
            {
                var adjPos = adj + center;
                var cell = GetCellAt(adjPos);
                if (cell != null)
                    action(cell, adjPos);
            }
        }

        public IEnumerable<CellState> AllCells
        {
            get
            {
                for (int i = 0; i < FieldDesc.Rows; ++i)
                    for (int j = 0; j < FieldDesc.Columns; ++j)
                        yield return GetCellAt(new Coord(i, j));
            }
        }
    }

    public static class MineFieldExtensions
    {
        public static T GetCellAt<T>(this T[,] array, Coord coord)
        {
            if (coord.Row < 0 || coord.Row >= array.GetLength(0)
                || coord.Column < 0 || coord.Column >= array.GetLength(1))
                return default(T);

            return array[coord.Row, coord.Column];
        }
    }
}
