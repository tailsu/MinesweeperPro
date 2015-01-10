using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MinesweeperPro.Mines;
using MinesweeperPro.Mines.Autoplay;

namespace MinesweeperPro
{
    public partial class MainWindow : Window, IThoughtLogger
    {
        private readonly DispatcherTimer myMoveTimer = new DispatcherTimer();

        private IAutoPlayer myPlayer;
        private Thought mySelectedThought;

        public MineField MineField { get; set; }

        public MainWindow()
        {
            this.Thoughts = new ObservableCollection<Thought>();
            this.DataContext = this;

            this.MineField = new MineField();
            this.MineField.GameOver += MineFieldOnGameOver;

            myPlayer = new ExpertAutoPlayer(Strategies.Auto);

            myMoveTimer.Interval = TimeSpan.FromSeconds(0.0);
            myMoveTimer.Tick += (sender, args) => myPlayer.DoNextMove();

            InitializeComponent();

            Restart();
        }

        private void MineFieldOnGameOver(object sender, GameOverEventArgs eventArgs)
        {
            Think(eventArgs.Detonated ? "Busted..." : "And we're done!");
            myMoveTimer.Stop();
        }

        public class RowAdapter
        {
            private readonly MineField myField;
            private readonly int myRow;
            public IEnumerable<CellState> Cells
            {
                get
                {
                    for (int i = 0; i < myField.FieldDesc.Columns; ++i)
                        yield return myField.Field[myRow, i];
                }
            }

            public RowAdapter(MineField field, int row)
            {
                myField = field;
                myRow = row;
            }
        }

        public IEnumerable<RowAdapter> Rows
        {
            get
            {
                for (int i = 0; i < MineField.FieldDesc.Rows; ++i)
                    yield return new RowAdapter(MineField, i);
            }
        }

        public class Thought
        {
            public string Content { get; set; }
            public MineFieldMemento State { get; set; }
        }

        public ObservableCollection<Thought> Thoughts { get; private set; }

        public Thought SelectedThought
        {
            get { return mySelectedThought; }
            set
            {
                mySelectedThought = value;
                if (value != null && !myMoveTimer.IsEnabled)
                {
                    MineField.RecreateField(mySelectedThought.State);
                    RebindField();
                }
            }
        }

        public void Think(string thought, params object[] args)
        {
            var str = String.Format(thought, args);
            Thoughts.Add(new Thought
                {
                    Content = str,
                    State = new MineFieldMemento(MineField),
                });
        }

        private void RestartClicked(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        private void Restart()
        {
            myMoveTimer.Stop();

            Thoughts.Clear();

            var builder = new RandomMineFieldBuilder {Desc = MineFieldDesc.Expert};
            this.MineField.RecreateField(builder);
            myPlayer.StartGame(MineField, this);
            myMoveTimer.Start();

            RebindField();
        }

        private void RebindField()
        {
            this.DataContext = null;
            this.DataContext = this;
        }
    }
}
