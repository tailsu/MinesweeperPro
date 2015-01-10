namespace MinesweeperPro.Mines
{
    public class MineFieldDesc
    {
        public static readonly MineFieldDesc Beginner = new MineFieldDesc(9, 9, 10);
        public static readonly MineFieldDesc Intermediate = new MineFieldDesc(16, 16, 40);
        public static readonly MineFieldDesc Expert = new MineFieldDesc(16, 30, 99);

        public readonly int Rows;
        public readonly int Columns;
        public readonly int? Mines;

        public MineFieldDesc(int rows, int columns, int? mines)
        {
            Rows = rows;
            Columns = columns;
            Mines = mines;
        }
    }
}