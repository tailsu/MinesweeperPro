namespace MinesweeperPro.Mines.Autoplay
{
    public interface IThoughtLogger
    {
        void Think(string thought, params object[] args);
    }
}
