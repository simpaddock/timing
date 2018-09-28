
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveTiming;
using Nancy.Hosting.Self;
using rF2SMMonitor;
using rF2SMMonitor.rFactor2Data;

namespace LiveTiming
{
    class Program
    {
        public static MappedBuffer<rF2Telemetry> telemetryBuffer = new MappedBuffer<rF2Telemetry>(rFactor2Constants.MM_TELEMETRY_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        public static MappedBuffer<rF2Scoring> scoringBuffer = new MappedBuffer<rF2Scoring>(rFactor2Constants.MM_SCORING_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        public static MappedBuffer<rF2Rules> rulesBuffer = new MappedBuffer<rF2Rules>(rFactor2Constants.MM_RULES_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        public static MappedBuffer<rF2Extended> extendedBuffer = new MappedBuffer<rF2Extended>(rFactor2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);

        public static rF2Telemetry telemetry = new rF2Telemetry();
        public static rF2Scoring scoring = new rF2Scoring();
        public static rF2Rules rules = new rF2Rules();
        public static rF2Extended extended = new rF2Extended();

        public static ApiResponse lastResponse;
        static void Main(string[] args)
        {
            telemetryBuffer.Connect();
            scoringBuffer.Connect();
            rulesBuffer.Connect();
            extendedBuffer.Connect();
            Timing timing = new Timing();

            using (var host = new NancyHost(new Uri("http://localhost:8080")))
            {
                host.Start();
                Console.ReadLine();
            }

            telemetryBuffer.Disconnect();
            scoringBuffer.Disconnect();
            rulesBuffer.Disconnect();
            extendedBuffer.Disconnect();

        }
    }
}
