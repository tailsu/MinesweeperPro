using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinesweeperPro.Mines;
using MinesweeperPro.Mines.Autoplay;

namespace MinesweeperTest
{
    [TestClass]
    public class ExpertAutoPlayerTests
    {
        public class Fixture
        {
            public MineField Field { get; private set; }
            public ExpertAutoPlayer Player { get; private set; }
            public bool? Completed { get; private set; }

            public Fixture(IMineFieldBuilder board, bool allowRandomMoves = false)
            {
                Field = new MineField();
                Field.RecreateField(board);
                Field.GameOver += FieldOnGameOver;

                Player = new ExpertAutoPlayer(allowRandomMoves ? Strategies.Auto : Strategies.AllDeterministic);
                Player.StartGame(Field, null);
            }

            private void FieldOnGameOver(object sender, GameOverEventArgs eventArgs)
            {
                Completed = !eventArgs.Detonated;
            }

            public void DoMoves(int count)
            {
                for (int i = 0; i < count && Completed == null; ++i)
                    Player.DoNextMove();
            }

            public string CurrentBoardState
            {
                get
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < Field.FieldDesc.Rows; ++i)
                    {
                        for (int j = 0; j < Field.FieldDesc.Columns; ++j)
                        {
                            var cell = Field.GetCellAt(i, j);
                            if (cell.IsFlagged && cell.HasMine)
                                sb.Append(PredefinedBoard.FlaggedMine);
                            else if (cell.IsFlagged)
                                sb.Append(PredefinedBoard.FlaggedWrong);
                            else if (cell.IsOpen)
                            {
                                if (cell.AdjacentMines == 0)
                                    sb.Append(PredefinedBoard.EmptyNoAdjacent);
                                else sb.Append(cell.AdjacentMines);
                            }
                            else if (cell.HasMine)
                                sb.Append(PredefinedBoard.Mine);
                            else sb.Append(PredefinedBoard.CoveredEmpty);
                        }
                        sb.AppendLine();
                    }
                    return sb.ToString();
                }
            }
        }

        [TestMethod]
        public void TestExhaustedCell1()
        {
            var board = new PredefinedBoard()
                .Row("*1")
                .Row("11");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Field.GetCellAt(0, 0).IsFlagged);
        }

        [TestMethod]
        public void TestExhaustedCell8()
        {
            var board = new PredefinedBoard()
                .Row("***")
                .Row("*8*")
                .Row("***");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    var cell = f.Field.GetCellAt(i, j);
                    if (cell.HasMine)
                        Assert.IsTrue(cell.IsFlagged);
                }
            }
        }

        [TestMethod]
        public void TestOpenDeterminedCell()
        {
            var board = new PredefinedBoard()
                .Row("...")
                .Row("!4.")
                .Row("!!!");
            var f = new Fixture(board);
            f.Player.DoNextMove();
            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestSupersetExclusion121()
        {
            var board = new PredefinedBoard()
                .Row("*.*")
                .Row("121");
            var f = new Fixture(board);
            f.DoMoves(3);
            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestSupersetExclusionMarkingConstrainedSinglePair()
        {
            var board = new PredefinedBoard()
                .Row("***")
                .Row(".5.")
                .Row("*2*")
                .Row("...");
            var f = new Fixture(board);
            f.DoMoves(4);

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestSupersetExclusionOpeningConstrainedSinglePair()
        {
            var board = new PredefinedBoard()
                .Row("...")
                .Row("*2.")
                .Row(".2*");
            var f = new Fixture(board);
            f.DoMoves(4);

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestSupersetExclusionOpeningBulgingOnes()
        {
            var board = new PredefinedBoard()
                .Row(".......")
                .Row("*......")
                .Row("...*.*.")
                .Row("*11122*");
            var f = new Fixture(board);
            f.DoMoves(12);

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestSupersetExclusionOnes()
        {
            var board = new PredefinedBoard()
                .Row("1.1")
                .Row("1*1")
                .Row("1.1");
            var f = new Fixture(board);
            f.DoMoves(3);

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestOpenSupersetExclusionFromTwoNonIntersectingPairs()
        {
            var board = new PredefinedBoard()
                .Row("!...!")
                .Row("3*2.3")
                .Row("!..*!");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Field.GetCellAt(0, 2).IsOpen);
            Assert.IsTrue(f.Field.GetCellAt(2, 2).IsOpen);
        }

        [TestMethod]
        public void TestMarkSupersetExclusionFromTwoNonIntersectingPairs()
        {
            var board = new PredefinedBoard()
                .Row("!.*.!")
                .Row("3*4.3")
                .Row("!.**!");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Field.GetCellAt(0, 2).IsFlagged);
            Assert.IsTrue(f.Field.GetCellAt(2, 2).IsFlagged);
        }

        [TestMethod]
        public void TestRecursiveSolutionFinding()
        {
            var board = new PredefinedBoard()
                .Row("..**..")
                .Row(".2222.")
                .Row("*2  2*")
                .Row("*2  2*")
                .Row(".2222.")
                .Row("..**..");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestMinePositionInferenceFromRemainingMinesCount()
        {
            var board = new PredefinedBoard()
                .Row("13!")
                .Row(".*!")
                .Row("..4")
                .Row(".*!")
                .Row("13!");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestEmptyCellInferenceFromRemainingMinesCount1()
        {
            var board = new PredefinedBoard()
                .Row("13!")
                .Row("*.!")
                .Row(".*2")
                .Row("..1")
                .Row("..2")
                .Row(".*!")
                .Row("13!");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestEmptyCellInferenceFromRemainingMinesCount2()
        {
            var board = new PredefinedBoard()
                .Row("13!")
                .Row("*.!")
                .Row("..2")
                .Row(".*1")
                .Row("..2")
                .Row("*.!")
                .Row("13!");
            var f = new Fixture(board);
            f.Player.DoNextMove();

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestRecursiveSupersetExclusion()
        {
            //TODO: the test must not be solvable by exhaustive recursive solving
            var board = new PredefinedBoard()
                .Row(".2!")
                .Row("..*")
                .Row("*4.")
                .Row("!3!");
            var f = new Fixture(board);
            f.DoMoves(4);

            Assert.IsTrue(f.Completed == true);
        }

        [TestMethod]
        public void TestLeastRiskCellOpening()
        {
            Assert.Inconclusive();
        }
    }
}
