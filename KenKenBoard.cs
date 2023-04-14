using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mathdoku
{
    public class KenKenBoard
    {
        public CellData[][] BoardData { get; private set; }
        public KenKenOptions Options { get; private set; }

        public KenKenBoard(KenKenOptions options)
        {
            Options = options;
            BoardData = GenerateBoard(options);
        }

        #region private
        private static CellData[][] GenerateBoard(KenKenOptions options)
        {
            Random rand = new Random();

            // generate a board in 4 phases
            //   1. create a board with 1..n unique values per row and column
            //   2. assign regions
            //   3. assign mathematical operators
            //   4. assign details and group ids

            // create the board data
            var boardData = new CellData[options.Dimension][];
            for (int y = 0; y < boardData.Length; y++)
            {
                boardData[y] = new CellData[options.Dimension];
                for (int x = 0; x < boardData[y].Length; x++)
                    boardData[y][x] = new CellData();
            }

            //
            // Phase 1 - create a constrained board
            // 
            var board = ConstrainedGridGenerator.Generate(options.Dimension, options.Seed);

            //
            // Phase 2 - assign regions
            //
            var slots = new List<int>();
            var regions = new Dictionary<int, RegionInfo>();

            // create all the slots
            for (int i = 0; i < board.Length * board[0].Length; i++) slots.Add(i);

            // randomly sort the list
            for (int i = 0; i < board.Length * board[0].Length; i++)
            {
                // swap with a random position
                var i2 = rand.Next() % (board.Length * board[0].Length);
                var ts = slots[i];
                slots[i] = slots[i2];
                slots[i2] = ts;
            }

            // start to create the board by assigning regions
            var nextRegion = 0;
            foreach (int slot in slots)
            {
                // get locator
                var y = slot / board.Length;
                var x = slot % board[0].Length;

                foreach (int r in ChooseRegion(boardData, y, x, rand, ref nextRegion))
                {
                    // assign to an existing region
                    if (regions.TryGetValue(r, out RegionInfo reg))
                    {
                        // check if the region is not already too big
                        if (reg.Values.Count >= LargestRegion(boardData, rand)) continue;
                    }
                    else
                    {
                        // create a new region
                        reg = new RegionInfo();
                        reg.ID = r;
                        regions.Add(r, reg);
                    }

                    // add the value to this region
                    reg.Values.Add(board[y][x]);

                    // assign the regionID
                    boardData[y][x].RegionID = r;
                    break;
                }

                // no region chosen - mark it as a new region
                if (boardData[y][x].RegionID == 0)
                {
                    var reg = new RegionInfo();
                    reg.ID = nextRegion++;
                    reg.Values.Add(board[y][x]);

                    // assign regionID
                    boardData[y][x].RegionID = reg.ID;
                    regions.Add(reg.ID, reg);
                }
            }

            //
            // Phase 3 - assign mathematical operators
            //
            // associate an operator for each region
            foreach (var reg in regions.Values)
            {
                if (reg.Op == Operator.None)
                {
                    var ops = ChooseOperator(reg, options, rand);

                    if (ops.Count == 0) throw new Exception("ASSERT: Catastrophic failure - no operations for this element");

                    // choose the first operator
                    foreach (Operator op in ops.Keys)
                    {
                        reg.Op = op;
                        switch (op)
                        {
                            case Operator.Solo: reg.Sign = ' '; break;
                            case Operator.Add: reg.Sign = '+'; break;
                            case Operator.Sub: reg.Sign = '-'; break;
                            case Operator.Mul: reg.Sign = 'x'; break;
                            case Operator.Div: reg.Sign = '÷'; break;
                        }
                        reg.Total = ops[op];
                        break;
                    }
                }
            }

            //
            // Phase 4 - assign details and group ids
            //
            // assign a GroupID that follows these rules:
            //  1. value 0-9
            //  2. does not overlap with a neighboring value

            // gather regions which are neighbors
            var neighbors = new Dictionary<int, HashSet<int>>();
            for (int y = 0; y < boardData.Length; y++)
            {
                for (int x = 0; x < boardData[y].Length; x++)
                {
                    if (!neighbors.TryGetValue(boardData[y][x].RegionID, out HashSet<int> values))
                    {
                        values = new HashSet<int>();
                        neighbors[boardData[y][x].RegionID] = values;
                    }

                    // check if the region above is different
                    if (x - 1 >= 0 && boardData[y][x - 1].RegionID != boardData[y][x].RegionID) values.Add(boardData[y][x - 1].RegionID);

                    // check if the region to the left is different
                    if (y - 1 >= 0 && boardData[y - 1][x].RegionID != boardData[y][x].RegionID) values.Add(boardData[y - 1][x].RegionID);

                    // check if the region to the right is different
                    if (x + 1 < boardData[y].Length && boardData[y][x + 1].RegionID != boardData[y][x].RegionID) values.Add(boardData[y][x + 1].RegionID);

                    // check if the region below is different
                    if (y + 1 < boardData.Length && boardData[y + 1][x].RegionID != boardData[y][x].RegionID) values.Add(boardData[y + 1][x].RegionID);
                }
            }

            // assign details and group ids (which are the same for each region)
            var regionIdToGroupId = new Dictionary<int, int>();
            for (int y = 0; y < boardData.Length; y++)
            {
                for (int x = 0; x < boardData[y].Length; x++)
                {
                    // match a group id based on previously seen regions
                    if (!regionIdToGroupId.TryGetValue(boardData[y][x].RegionID, out int groupId))
                    {
                        // assign a group id that is not a neighbor
                        var groupIds = new HashSet<int>();
                        foreach(var rid in neighbors[boardData[y][x].RegionID])
                        {
                            // add the neighbors assigned group ids
                            if (regionIdToGroupId.TryGetValue(rid, out groupId))
                            {
                                groupIds.Add(groupId);
                            }
                        }

                        // choose from a group other than the ones already present
                       groupId = 0;
                       while (groupIds.Contains(groupId)) groupId++;

                        // assign this group id a region
                        regionIdToGroupId.Add(boardData[y][x].RegionID, groupId);
                    }

                    // fill in details
                    boardData[y][x].Value = board[y][x];
                    boardData[y][x].Total = regions[boardData[y][x].RegionID].Total;
                    boardData[y][x].Operator = regions[boardData[y][x].RegionID].Sign;
                    boardData[y][x].GroupID = groupId;
                }
            }

            return boardData;
        }

        private static int LargestRegion(CellData[][] boardData, Random rand)
        {
            switch (boardData.Length)
            {
                case 6:
                    return (rand.Next() % 8 == 0) ? 4 : 3;
                case 4:
                    return (rand.Next() % 4 == 0) ? 4 : 3;
                default:
                    return boardData.Length - 2;
            }
        }

        private static List<int> ChooseRegion(CellData[][] boardData, int y, int x, Random rand, ref int nextRegion)
        {
            // find a neighbor and choose that value
            var regions = new List<int>();

            // choose all 4 directions
            if (x - 1 >= 0 && boardData[y][x - 1].RegionID > 0) regions.Add(boardData[y][x - 1].RegionID);
            if (y - 1 >= 0 && boardData[y - 1][x].RegionID > 0) regions.Add(boardData[y - 1][x].RegionID);
            if (x + 1 < boardData[y].Length && boardData[y][x + 1].RegionID > 0) regions.Add(boardData[y][x + 1].RegionID);
            if (y + 1 < boardData.Length && boardData[y + 1][x].RegionID > 0) regions.Add(boardData[y + 1][x].RegionID);

            // randomly introduce a new potential region into the mix
            if (rand.Next() % 4 == 0) regions.Add(nextRegion++);

            // randomize the order
            for (int i = 0; i < regions.Count; i++)
            {
                var i2 = rand.Next() % regions.Count;
                var r = regions[i];
                regions[i] = regions[i2];
                regions[i2] = r;
            }

            return regions;
        }

        class AscendingComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x == y) return 0; // equal
                if (x < y) return 1; // y greater
                return -1; // x greater
            }
        }

        private static bool ComputeAllMathematics(List<int> values, out int add, out int mul, out int sub, out int div)
        {
            bool containsOnes = false;

            // sort the value HIGHEST to LOWEST for Sub and Div
            AscendingComparer a = new AscendingComparer();
            values.Sort(a);

            // calculate
            add = 0;
            mul = 1;
            sub = Int32.MaxValue;
            div = Int32.MaxValue;
            foreach (int val in values)
            {
                if (val == 1) containsOnes = true;

                add += val;

                mul *= val;

                if (sub == Int32.MaxValue) sub = val;
                else sub -= val;

                if (div == Int32.MaxValue) div = val;
                else if (div > 0 && val > 0)
                {
                    if (div % val == 0) div = div / val;
                    else div = -1;
                }
                else
                    div = -1;
            }

            return containsOnes;
        }

        private static Dictionary<Operator, int> ChooseOperator(RegionInfo reg, KenKenOptions options, Random rand)
        {
            var ops = new Dictionary<Operator, int>();

            if (reg.Values.Count == 1)
            {
                ops.Add(Operator.Solo, reg.Values[0]);
                return ops;
            }

            // calculate all the possible mathematical operations
            var containsOnes = ComputeAllMathematics(reg.Values, out int add, out int mul, out int sub, out int div);

            // add in order of priority to be chosen
            // check if the cell is either 2 (with Associative enabled) or if Associative is off then anything is fine
            if ((options.IsAssociative && reg.Values.Count == 2) || !options.IsAssociative)
            {
                // prioritize division and subtraction in regions of size 2
                if (rand.Next() % 4 == 0)
                {
                    if (sub > 0) ops.Add(Operator.Sub, sub);
                    if (div > 1) ops.Add(Operator.Div, div);
                }
                else
                {
                    if (div > 1) ops.Add(Operator.Div, div);
                    if (sub > 0) ops.Add(Operator.Sub, sub);
                }
            }

            // include addition and multiplication
            if (rand.Next() % 2 == 0)
            {
                ops.Add(Operator.Add, add);
                if (!containsOnes) ops.Add(Operator.Mul, mul);
            }
            else
            {
                if (!containsOnes) ops.Add(Operator.Mul, mul);
                ops.Add(Operator.Add, add);
            }

            return ops;
        }
    }
    #endregion
}
