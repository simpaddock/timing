using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveTiming
{
    static class Constants
    {
        public static String[] PitStates =
        {
            "None",
            "Request",
            "Entering",
            "Stopped",
            "Exiting"
        };
        public static String[] Status =
        {
            "None",
            "Finished",
            "DNF",
            "DQ"
        };
    }
    class Entry
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String TeamName { get; set; }
        public int Position { get; set; }
        public String EntryClass { get; set; }
        public String FrontTires { get; set; }
        public String RearTires { get; set; }
        public String PitState { get; set; }
        public double TimeBehind { get; set; }
        public int LapsBehind { get; set; }
        public int Stops { get; set; }
        public int Laps { get; set; }
        public String Status { get; set; }
        public int PositionDifference { get; set; }
        public double[] LastSectorTimes { get; set; }
        public double[] BestSectorTimes { get; set; }
        public double BestLap { get; set; }
        public double LastLap { get; set; }
        public String VehicleName { get; set; }
        public int Number { get; set; }
    }
}
