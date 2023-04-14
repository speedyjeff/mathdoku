using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    enum Operator { None, Solo, Add, Sub, Mul, Div };

    class RegionInfo
    {
        public int ID { get; set; }
        public List<int> Values { get; set; }
        public int Total { get; set; }
        public Operator Op { get; set; }
        public char Sign { get; set; }

        public RegionInfo()
        {
            ID = -1;
            Values = new List<int>();
            Total = -1;
            Op = Operator.None;
            Sign = ' ';
        }
    }
}
