using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    class CellHistory
    {
        public int X { get; set; }
        public int Y { get; set; }
        // previous state of the cell
        public List<int> Before { get; set; }
        // new state of the cell
        public List<int> After { get; set; } 
    }
}
