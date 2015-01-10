using System;
using System.Diagnostics;

namespace MinesweeperPro.Mines
{
    [DebuggerDisplay("({Row},{Column})")]
    public struct Coord : IEquatable<Coord>
    {
        public readonly int Row;
        public readonly int Column;

        public Coord(int row, int column) : this()
        {
            Row = row;
            Column = column;
        }

        public static Coord operator + (Coord a, Coord b)
        {
            return new Coord(a.Row + b.Row, a.Column + b.Column);
        }

        public static double ManhattanDistance(Coord a, Coord b)
        {
            return Math.Abs(b.Column - a.Column) + Math.Abs(b.Row - a.Row);
        }

        #region Equality members

        public bool Equals(Coord other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Coord && Equals((Coord) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Row << 16) | Column;
            }
        }

        #endregion
    }
}
