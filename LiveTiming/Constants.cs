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
        public static String PADDOCKURL = "http://paddock.0fury.de/api/entries/2";
        public static int MAXTIMECOUNTDOWN = 30;
    }
}
