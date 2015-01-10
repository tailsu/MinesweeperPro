using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesweeperPro.Mines.Autoplay
{
    [Flags]
    public enum Strategies
    {
        Basic = 1,
        Advanced = 2,
        Expert = 4,
        Risky = 8,

        AllDeterministic = Basic | Advanced | Expert,
        Auto = AllDeterministic | Risky,
    }

    public class ExpertAutoPlayer : IAutoPlayer
    {
        private interface IMove
        {
            string Explanation { get; }
            Coord Center { get; }
            void DoMove();
        }

        private interface IMoveStrategy
        {
            IEnumerable<IMove> FindMoves(MineField field);
        }

        private MineField myField;
        private IThoughtLogger myThoughtLogger;
        private Coord myLastMove;

        private List<List<IMoveStrategy>> myStrategies = new List<List<IMoveStrategy>>();

        public ExpertAutoPlayer(Strategies allowedStrategies)
        {
            if (allowedStrategies.HasFlag(Strategies.Basic))
            {
                myStrategies.Add(new List<IMoveStrategy>
                    {new DeterminedCellsOpener(), new ExhaustedAdjacentCellMarker()});
            }

            if (allowedStrategies.HasFlag(Strategies.Advanced))
            {
                myStrategies.Add(new List<IMoveStrategy> {new MineConstrainedSupersetFinder()});
            }

            if (allowedStrategies.HasFlag(Strategies.Expert))
            {
                myStrategies.Add(new List<IMoveStrategy> {new ExhaustiveRecursiveSolver()});
            }

            if (allowedStrategies.HasFlag(Strategies.Risky))
            {
                var riskyStrategies = new List<IMoveStrategy>();
                riskyStrategies.Add(new UninformedRandomGuess());
                myStrategies.Add(riskyStrategies);
            }
        }

        public void StartGame(MineField field, IThoughtLogger thoughtLogger)
        {
            myField = field;
            myThoughtLogger = thoughtLogger;
        }

        public void DoNextMove()
        {
            int strategyIdx = 0;
            foreach (var strategySet in myStrategies)
            {
                var moves = new List<IMove>();
                foreach (var strategy in strategySet)
                    moves.AddRange(strategy.FindMoves(myField));

                var bestMove = moves.OrderBy(m => Coord.ManhattanDistance(m.Center, myLastMove)).ToList();

                var move = bestMove.FirstOrDefault();
                if (move != null)
                {
                    if (myThoughtLogger != null)
                        myThoughtLogger.Think(move.Explanation);

                    myLastMove = move.Center;
                    move.DoMove();
                    return;
                }

                strategyIdx++;
            }
        }

        private class DeterminedCellMove : IMove
        {
            private readonly MineField myField;

            public DeterminedCellMove(Coord center, MineField field)
            {
                Center = center;
                myField = field;
            }

            public Coord Center { get; private set; }

            public string Explanation
            {
                get { return string.Format("Basic: Opening around determined cell: ({0},{1})", Center.Row, Center.Column); }
            }

            public void DoMove()
            {
                myField.ForEachAdjacency(Center, (cell, coord) =>
                    {
                        if (!cell.IsOpen && !cell.IsFlagged)
                            cell.Open();
                    });
            }
        }

        private class DeterminedCellsOpener : IMoveStrategy
        {
            public IEnumerable<IMove> FindMoves(MineField field)
            {
                var determinedCells = new List<Coord>();

                foreach (var cell in field.AllCells)
                {
                    if (cell.IsOpen && cell.AdjacentMines > 0)
                    {
                        if (cell.OpenedAdjacent < cell.SafeNeighbours && cell.FlaggedAdjacent == cell.AdjacentMines)
                        {
                            determinedCells.Add(cell.Coord);
                        }
                    }
                }

                return determinedCells.Select(cell => new DeterminedCellMove(cell, field)).Cast<IMove>();
            }  
        }

        private class MarkExhaustedAdjacentCellsMove : IMove
        {
            private MineField myField;

            public MarkExhaustedAdjacentCellsMove(Coord center, MineField field)
            {
                Center = center;
                myField = field;
            }

            public Coord Center { get; private set; }

            public string Explanation
            {
                get { return string.Format("Basic: Marking around exhausted cell: ({0},{1})", Center.Row, Center.Column); }
            }

            public void DoMove()
            {
                myField.ForEachAdjacency(Center, (cell, coord) =>
                {
                    if (!cell.IsOpen && !cell.IsFlagged)
                        cell.Flag();
                });
            }
        }

        private class ExhaustedAdjacentCellMarker : IMoveStrategy
        {
            public IEnumerable<IMove> FindMoves(MineField field)
            {
                var exhaustedCells = new List<Coord>();

                foreach (var cell in field.AllCells)
                {
                    if (cell.IsOpen && cell.AdjacentMines > 0)
                    {
                        if (cell.OpenedAdjacent == cell.SafeNeighbours && cell.FlaggedAdjacent < cell.AdjacentMines)
                        {
                            exhaustedCells.Add(cell.Coord);
                        }
                    }
                }

                return exhaustedCells.Select(cell => new MarkExhaustedAdjacentCellsMove(cell, field));
            }
        }

        private class MineConstrainedSupersetFinder : IMoveStrategy
        {
            public IEnumerable<IMove> FindMoves(MineField field)
            {
                int rows = field.FieldDesc.Rows;
                int columns = field.FieldDesc.Columns;
                var governedCells = new Tuple<CellState, HashSet<Coord>>[rows, columns];

                foreach (var cell in field.AllCells)
                {
                    if (cell.IsOpen)
                    {
                        var thisGovCells = new HashSet<Coord>();
                        field.ForEachAdjacency(cell.Coord, (adjCell, coord) =>
                            {
                                if (!adjCell.IsOpen && !adjCell.IsFlagged)
                                    thisGovCells.Add(coord);
                            });

                        if (thisGovCells.Count > 0)
                            governedCells[cell.Coord.Row, cell.Coord.Column] = Tuple.Create(cell, thisGovCells);
                    }
                }

                for (int srcRow = 0; srcRow < rows; ++srcRow)
                {
                    for (int srcColumn = 0; srcColumn < columns; ++srcColumn)
                    {
                        var srcCoord = new Coord(srcRow, srcColumn);
                        var tuple = governedCells.GetCellAt(srcCoord);
                        if (tuple == null)
                            continue;

                        var interactingCells = new List<CellState>();
                        for (int rowOffset = -2; rowOffset <= 2; ++rowOffset)
                        {
                            for (int columnOffset = -2; columnOffset <= 2; ++columnOffset)
                            {
                                if (rowOffset == 0 && columnOffset == 0)
                                    continue;

                                var coord = new Coord(rowOffset, columnOffset) + srcCoord;
                                var interactingCell = governedCells.GetCellAt(coord);
                                if (interactingCell != null)
                                    interactingCells.Add(interactingCell.Item1);
                            }
                        }

                        var srcCell = tuple.Item1;
                        var srcGoverned = tuple.Item2;

                        foreach (var interactingSubset in interactingCells.AllSubsetsFast(true, false))
                        {
                            int totalGovernedCells = 0;
                            var adjGovernedCellsUnion = new HashSet<Coord>();
                            int combinedAdjacentMines = 0;
                            foreach (var element in interactingSubset)
                            {
                                var elementGov = governedCells.GetCellAt(element.Coord);
                                var governedAdjacencies = elementGov.Item2;
                                totalGovernedCells += governedAdjacencies.Count;
                                adjGovernedCellsUnion.UnionWith(governedAdjacencies);
                                combinedAdjacentMines += elementGov.Item1.UnflaggedAdjacent;
                            }

                            // explore only subsets with non-intersecting governed cell sets
                            if (totalGovernedCells != adjGovernedCellsUnion.Count)
                                continue;

                            var srcUnique = srcGoverned.Except(adjGovernedCellsUnion).ToList();
                            var intersection = srcGoverned.Intersect(adjGovernedCellsUnion);
                            if (intersection.Any())
                            {
                                const string explanation = "excluded superset cells";
                                string level = interactingSubset.Count == 1 ? "Advanced" : "More advanced";

                                if (srcUnique.Count > 0 && srcUnique.Count == srcCell.UnflaggedAdjacent - combinedAdjacentMines)
                                    yield return new MarkSelectedCellsMove(srcUnique.Select(field.GetCellAt), srcCell.Coord, level, explanation);

                                if (srcCell.UnflaggedAdjacent == combinedAdjacentMines && srcGoverned.IsProperSupersetOf(adjGovernedCellsUnion))
                                    yield return new OpenSelectedCellsMove(srcUnique.Select(field.GetCellAt), srcCell.Coord, level, explanation);
                            }
                        }
                    }
                }
            }
        }

        private class MarkSelectedCellsMove : IMove
        {
            private readonly IEnumerable<CellState> myCellsToMark;
            private readonly string myLevel;
            private readonly string myExplanation;

            public MarkSelectedCellsMove(IEnumerable<CellState> cellsToMark, Coord center, string level, string explanation)
            {
                Center = center;
                myCellsToMark = cellsToMark.ToList();
                myLevel = level;
                myExplanation = explanation;
            }

            public Coord Center { get; private set; }

            public string Explanation
            {
                get { return string.Format("{2}: Marking {3} - ({0},{1})", Center.Row, Center.Column, myLevel, myExplanation); }
            }

            public void DoMove()
            {
                foreach (var cell in myCellsToMark)
                {
                    if (!cell.IsOpen && !cell.IsFlagged)
                        cell.Flag();
                };
            }
        }

        private class OpenSelectedCellsMove : IMove
        {
            private readonly IEnumerable<CellState> myCellsToOpen;
            private readonly string myLevel;
            private readonly string myExplanation;

            public OpenSelectedCellsMove(IEnumerable<CellState> cellsToOpen, Coord center, string level, string explanation)
            {
                Center = center;
                myCellsToOpen = cellsToOpen.ToList();
                myLevel = level;
                myExplanation = explanation;
            }

            public Coord Center { get; private set; }

            public string Explanation
            {
                get { return string.Format("{2}: Opening {3} - ({0},{1})", Center.Row, Center.Column, myLevel, myExplanation); }
            }

            public void DoMove()
            {
                foreach (var cell in myCellsToOpen)
                {
                    if (!cell.IsOpen && !cell.IsFlagged)
                        cell.Open();
                };
            }
        }

        private class UninformedRandomGuess : IMoveStrategy
        {
            public IEnumerable<IMove> FindMoves(MineField field)
            {
                var freeCells = field.AllCells
                    .Where(cell => cell.InterfaceState == CellInterfaceState.Unopened)
                    .ToList();

                var clickHere = freeCells.Choose();

                yield return new OpenSelectedCellsMove(
                    new[] { clickHere }, clickHere.Coord, "Risky (basic)", "at random");
            }
        }

        private class ExhaustiveRecursiveSolver : IMoveStrategy
        {
            private CellState[,] myCells;

            public IEnumerable<IMove> FindMoves(MineField field)
            {
                var rows = field.FieldDesc.Rows;
                var cols = field.FieldDesc.Columns;
                myCells = new CellState[rows, cols];
                for (int i = 0; i < rows; ++i)
                    for (int j = 0; j < cols; ++j)
                    {
                        var cell = field.GetCellAt(i, j);
                        myCells[i, j] = new CellState(null, cell.Coord)
                            {
                                AdjacentMines = cell.IsOpen ? cell.AdjacentMines : -1,
                                InterfaceState = cell.InterfaceState,
                            };
                    }

                Recurse();

                yield break;
            }

            private void Recurse()
            {
                var determinedCells = new List<Coord>();
                foreach (var cell in myCells)
                {
                    if (cell.IsOpen && cell.AdjacentMines > 0)
                    {
                        if (cell.OpenedAdjacent < cell.SafeNeighbours && cell.FlaggedAdjacent == cell.AdjacentMines)
                        {
                            determinedCells.Add(cell.Coord);
                        }
                    }
                }

            }
        }
    }

    internal static class EnumerableExtensions
    {
        public static IEnumerable<IList<T>> AllSubsetsFast<T>(this IList<T> list, bool onlyNonEmpty, bool onlyProperSubsets)
        {
            if (list.Count > 32)
                throw new ArgumentException("list too large", "list");

            var upperBound = unchecked((1U << list.Count) - 1);
            if (onlyProperSubsets)
                upperBound--;

            uint variation = onlyNonEmpty ? 1U : 0;
            for (; variation <= upperBound; ++variation)
            {
                var sublist = list.Where((t, i) => ((1 << i) & variation) != 0).ToList();
                yield return sublist;
            }
        }

        public static IEnumerable<T> Flatten<T>(this T[,] array)
        {
            for (int i = 0; i < array.GetLength(0); ++i)
                for (int j = 0; j < array.GetLength(1); ++j)
                    yield return array[i, j];
        }
    }
}
