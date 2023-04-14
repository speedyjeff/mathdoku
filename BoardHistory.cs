using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    class BoardHistory
    {
        public BoardHistory()
        {
            History = new CellHistory[MaxHistoryLength];
            CurIndex = 0;
        }

        public void Clear()
        {
            CurIndex = 0;
            for(int i=0; i<History.Length; i++) History[i] = null;
        }

        public bool Add(int y, int x, List<int> before, List<int> after)
        {
            // increment index
            CurIndex = Increment(CurIndex);

            // set the current
            History[CurIndex] = new CellHistory()
            {
                X = x,
                Y = y,
                Before = new List<int>(),
                After = new List<int>()
            };
            foreach (int val in before) History[CurIndex].Before.Add(val);
            foreach (int val in after) History[CurIndex].After.Add(val);

            // clear the next to indicate TheBeginning
            History[Increment(CurIndex)] = null;

            return true;
        }

        public bool Redo(ref int y, ref int x, ref List<int> values)
        {
            if (History[Increment(CurIndex)] == null) return false;

            // advance
            CurIndex = Increment(CurIndex);

            // grab the data
            y = History[CurIndex].Y;
            x = History[CurIndex].X;
            foreach (int val in History[CurIndex].After) values.Add(val);

            return true;
        }

        public bool Undo(ref int y, ref int x, ref List<int> values)
        {
            if (History[CurIndex] == null) return false;

            // grab the data
            y = History[CurIndex].Y;
            x = History[CurIndex].X;
            foreach (int val in History[CurIndex].Before) values.Add(val);

            // decrement
            CurIndex = Decrement(CurIndex);

            return true;
        }

        #region private
        private CellHistory[] History;
        private const int MaxHistoryLength = 72; // each cell twice
        private int CurIndex;

        // at all times CurIndex points to the last valid history
        //   EXCEPT when the queue is empty
        // A null indicates the beginning of the list

        private int Increment(int index)
        {
            return (index + 1) % History.Length;
        }

        private int Decrement(int index)
        {
            if (index == 0) index = History.Length;
            return index - 1;
        }
        #endregion
    }
}
