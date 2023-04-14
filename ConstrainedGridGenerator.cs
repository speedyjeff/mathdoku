using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    static class ConstrainedGridGenerator
    {
        public static int[][] Generate(int dimension, int seed)
        {
            // create a valid KenKen (each row and column contain a unique value between 1 and dimension)

            // algorithm:
            //  - Randomize a list containing all possible grid locations.
            //  - Keep a history tree of all possible moves
            //  - If the board becomes unsolvable backup the tree

            var rand = new Random(seed);
            var board = new int[dimension][];
            var kTree = new KTree();
            var cNode = kTree.Head;
            var slots = new Stack<int>();

            for(var y = 0; y<board.Length; y++) board[y] =  new int[dimension];

            // add the slots in diagonals (bottom half)
            for (int i = 0; i < dimension; i++)
            {
                var y = i;
                var x = 0;
                while (0 <= y && dimension > x)
                {
                    slots.Push((y * dimension) + x);
                    y--;
                    x++;
                }
            }
            // add the slots in diagonals (top half)
            for (int i = 1; i < dimension; i++)
            {
                int y = dimension - 1;
                int x = i;
                while (0 <= y && dimension > x)
                {
                    slots.Push((y * dimension) + x);
                    y--;
                    x++;
                }
            }

            // place a number for each slot into the board (following the rules of unique per column/row)
            while (slots.Count > 0)
            {
                if (cNode == null)
                {
                    throw new Exception("Internal ERROR!  We traversed all the way back up to the head in error");
                }
                else if (cNode.children == null)
                {
                    BackupTheTree(kTree, ref cNode, board, slots, dimension);
                }
                else if (cNode.children.Count == 0)
                {
                    // find the possible children
                    var cSlot = slots.Peek();
                    var ycord = cSlot / dimension;
                    var xcord = cSlot % dimension;

                    // get the available values
                    var items = GetAvailableValues(board, ycord, xcord);

                    if (items.Count == 0)
                    {
                        // there are no possible numbers that can go into this location
                        var deadTreeSlots = GetAllRowColumnSlots(dimension, cNode);

                        // start walking backwards to find a better decision in the tree
                        while (cNode != null && deadTreeSlots.Contains(cNode.Slot))
                        {
                            if (cNode.Parent != null && deadTreeSlots.Contains(cNode.Parent.Slot))
                            {
                                cNode.Parent.children.Clear();
                            }
                            BackupTheTree(kTree, ref cNode, board, slots, dimension);
                        }
                    }
                    else
                    {
                        // mark this slot as used
                        slots.Pop();

                        // translate these items into children
                        foreach (var value in items)
                        {
                            var nNode = new KTreeNode()
                            {
                                Value = value,
                                Slot = cSlot,
                                Parent = cNode
                            };

                            kTree.AddChild(cNode, nNode);
                        }
                    }
                }

                // place a value on the board and choose it as the new head
                if (null != cNode && null != cNode.children)
                {
                    // randomly choose a child as the new head
                    cNode = cNode.children[rand.Next(0, cNode.children.Count)];

                    // add this child to the board
                    var cSlot = cNode.Slot;
                    var ycord = cSlot / dimension;
                    var xcord = cSlot % dimension;
                    board[ycord][xcord] = cNode.Value;
                }
            }

            return board;
        }

        #region private
        private static List<int> GetAvailableValues(int[][] board, int ycord, int xcord)
        {
            // get the set of available numbers
            List<int> items = new List<int>();

            // add all possible entries (eg. 1..6 - if the dim is 6)
            for (int i = 1; i <= board.Length; i++) items.Add(i);

            // row
            for (int i = 0; i < board[ycord].Length; i++) items.Remove(board[ycord][i]);
            // column
            for (int j = 0; j < board.Length; j++) items.Remove(board[j][xcord]);

            // return values that have not been used yet in this row/column
            return items;
        }

        private static HashSet<int> GetAllRowColumnSlots(int dimension, KTreeNode cNode)
        {
            // get the set of available numbers
            var ycord = cNode.Slot / dimension;
            var xcord = cNode.Slot % dimension;
            var items = new HashSet<int>();

            // row
            for (int i = 0; i < dimension; i++) items.Add((ycord * dimension) + i);
            // column
            for (int j = 0; j < dimension; j++) items.Add((j * dimension) + xcord);

            return items;
        }

        private static void BackupTheTree(KTree kTree, ref KTreeNode cNode, int[][] board, Stack<int> slots, int dimension)
        {
            // a dead end was found - back track up the tree
            var ycord = cNode.Slot / dimension;
            var xcord = cNode.Slot % dimension;
            var pNode = cNode;

            // clear the current choice from the board
            board[ycord][xcord] = 0;
            // identify the parent
            cNode = cNode.Parent;
            // remove this child
            kTree.RemoveChild(cNode, pNode);
            // if this was the last child available, push the slot back onto the stack
            if (cNode.children.Count == 0)
            {
                slots.Push(pNode.Slot);
                cNode.children = null;
            }
        }
        #endregion  
    }
}
