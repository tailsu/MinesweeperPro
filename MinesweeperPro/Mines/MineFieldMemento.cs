namespace MinesweeperPro.Mines
{
    public class MineFieldMemento : IMineFieldBuilder
    {
        private CellMemento[,] fieldState;

        private class CellMemento
        {
            public bool HasMine;
            public Coord Coord;
            public int AdjacentMines;
            public CellInterfaceState State;
        }

        public MineFieldMemento(MineField field)
        {
            Desc = field.FieldDesc;
            fieldState = new CellMemento[Desc.Rows, Desc.Columns];
            for (int i = 0; i < Desc.Rows; ++i)
                for (int j = 0; j < Desc.Columns; ++j)
                {
                    var cell = field.GetCellAt(i, j);
                    fieldState[i, j] = new CellMemento
                        {
                            AdjacentMines = cell.AdjacentMines,
                            Coord = cell.Coord,
                            HasMine = cell.HasMine,
                            State = cell.InterfaceState,
                        };
                }
        }

        public void BuildMineField(MineField mineField)
        {
            Field = new CellState[Desc.Rows, Desc.Columns];
            for (int i = 0; i < Desc.Rows; ++i)
                for (int j = 0; j < Desc.Columns; ++j)
                {
                    var memento = fieldState[i, j];
                    Field[i, j] = new CellState(mineField, memento.Coord)
                        {
                            AdjacentMines = memento.AdjacentMines,
                            HasMine = memento.HasMine,
                            InterfaceState = memento.State,
                        };
                }
        }

        public CellState[,] Field { get; private set; }
        public MineFieldDesc Desc { get; private set; }
    }
}
