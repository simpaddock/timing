using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveTiming
{
    class ApiResponse
    {
        public Session Session;
        public Entry[] Drivers;
        public String RaceOverlayControlSet; // JSON, because general purpose
        public int SlotId;
        public int CameraId;
        public int CommandId; // to see if something changed..
    }
}
