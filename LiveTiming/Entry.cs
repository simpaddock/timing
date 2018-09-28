using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveTiming
{
    class Entry
    {
        public int SlotID { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String TeamName { get; set; }
        public int Position { get; set; }
        public String EntryClass { get; set; }
        public String FrontTires { get; set; }
        public String RearTires { get; set; }
        public String PitState { get; set; }
        public double TimeBehind { get; set; }
        public string TimeBehindString { get; set; }
        public int LapsBehind { get; set; }
        public int Stops { get; set; }
        public int Laps { get; set; }
        public String Status { get; set; }
        public int PositionDifference { get; set; }
        public double[] LastSectorTimes { get; set; }
        public double[] BestSectorTimes { get; set; }
        public double BestLap { get; set; }
        public double LastLap { get; set; }
        public string LastLapString { get; set; }
        public string BestLapString { get; set; }
        public double BestLapDelta { get; set; }
        public string BestLapDeltaString { get; set; }
        public string CurrentLapString { get; set; }
        public String VehicleName { get; set; }
        public int Number { get; set; }
        public String NumberFormat { get; set; }
        public String FormattedNumber { get; set; }
        public int CurrentSessionPositionDifference { get; set; }

        // Problems
        public bool HasHeatingProblem { get; set;  }
        public bool HasLostParts { get; set; }
    }
}
