using System;

namespace MinesweeperPro.Mines
{
    public class CellState : Bindable
    {
        private readonly MineField myField;
        public readonly Coord Coord;

        private CellInterfaceState myInterfaceState;

        public bool HasMine { get; set; }
        public int AdjacentMines { get; set; }

        public int SafeNeighbours
        {
            get
            {
                bool onVertEdge = Coord.Column == 0 || Coord.Column == myField.FieldDesc.Columns - 1;
                bool onHorzEdge = Coord.Row == 0 || Coord.Row == myField.FieldDesc.Rows - 1;

                var totalNeighbors =
                    onHorzEdge && onVertEdge ? 3
                    : onHorzEdge || onVertEdge ? 5
                    : 8;

                return totalNeighbors - AdjacentMines;
            }
        }

        public bool IsOpen { get { return InterfaceState == CellInterfaceState.Opened; } }
        public bool IsFlagged { get { return InterfaceState == CellInterfaceState.Flagged; } }

        public CellState(MineField field, Coord coord)
        {
            myField = field;
            Coord = coord;
            myInterfaceState = CellInterfaceState.Unopened;
        }

        public CellInterfaceState InterfaceState
        {
            get { return myInterfaceState; }
            set { this.SetValue(ref myInterfaceState, value); }
        }

        public void Flag()
        {
            switch (InterfaceState)
            {
                case CellInterfaceState.Unopened:
                    InterfaceState = CellInterfaceState.Flagged;
                    myField.RemainingFlags--;
                    break;
                case CellInterfaceState.Opened:
                    break;
                case CellInterfaceState.Flagged:
                    InterfaceState = CellInterfaceState.Unopened;
                    myField.RemainingFlags++;
                    break;
            }
        }

        public void Open()
        {
            myField.Open(this.Coord);
        }

        private int GetAdjacentOfState(CellInterfaceState state)
        {
            int inState = 0;
            myField.ForEachAdjacency(Coord, (adjCell, adjCoord) =>
            {
                if (adjCell.InterfaceState == state)
                    inState++;
            });
            return inState;
        }

        public int OpenedAdjacent { get { return GetAdjacentOfState(CellInterfaceState.Opened); } }
        public int FlaggedAdjacent { get { return GetAdjacentOfState(CellInterfaceState.Flagged); } }
        public int UnflaggedAdjacent { get { return AdjacentMines - FlaggedAdjacent; } }
    }
}