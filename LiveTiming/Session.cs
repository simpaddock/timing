using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveTiming
{
    class Session
    {
        public Boolean[] YellowFlags { get; set; }
        public int MaxLaps { get; set; }
        public int CurrentLaps { get; set; }
        public double MaxTime { get; set; }
        public double CurrentTime { get; set; }
        public bool IsRace { get; set; }
    }
}
