using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    public class CellData
    {
        public int GroupID { get; set; }
        public int RegionID { get; set; }
        public int Value { get; set; }
        public int Total { get; set; }
        public char Operator { get; set; }
    }
}
