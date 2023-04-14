using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    public class KenKenOptions
    {
        public int Seed { get; set; }
        public string PuzzleName { get; set; }
        public int Dimension { get; set; }
        // true == sub/div are only 2 cells, false == 2 or 3 (default)
        public bool IsAssociative { get; set; }
        // percentage of cells that are filled in with hints
        public float StartingHint { get; set; }

        public static int PuzzleNameToSeed(string puzzleName)
        {
            if (String.IsNullOrWhiteSpace(puzzleName)) return -1;

            // first attempt to convert if it is a number
            if (Int32.TryParse(puzzleName, out int seed))
            {
                return seed;
            }

            // convert text to int
            seed = 0;
            foreach (char c in puzzleName.ToCharArray())
            {
                try
                {
                    seed += (int)c;
                }
                catch (Exception) // nasty catch all to guard against random user input
                {
                }
            }

            return Math.Abs(seed);
        }
    }
}
