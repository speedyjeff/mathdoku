using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mathdoku
{
    public enum NumberStates { On, Off, Disabled };

    public class UserInput
    {
        public int MinVal { get; set; }
        public int MaxVal { get; set; }

        public NumberStates[] States { get; set; }

        public int Y { get; set; }
        public int X { get; set; }

        public UserInput(int min, int max)
        {
            States = new NumberStates[min+max];
            for (int i = 0; i < States.Length; i++) States[i] = NumberStates.Off;
            MinVal = min;
            MaxVal = max;
        }
    }
}
