namespace MinesweeperPro.Mines
{
    public interface IMineFieldBuilder
    {
        void BuildMineField(MineField mineField);

        CellState[,] Field { get; }
        MineFieldDesc Desc { get; }
    }
}
