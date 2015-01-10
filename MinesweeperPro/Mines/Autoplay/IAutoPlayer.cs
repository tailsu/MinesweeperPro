namespace MinesweeperPro.Mines.Autoplay
{
    public interface IAutoPlayer
    {
        void StartGame(MineField field, IThoughtLogger thoughtLogger);
        void DoNextMove();
    }
}
