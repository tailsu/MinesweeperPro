using System;
using System.Collections.Generic;

namespace MinesweeperPro.Mines
{
    public static class Randomness
    {
        public static readonly Random RNG = new Random();

        public static T Choose<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[RNG.Next(list.Count)] : default(T);
        }
    }
}