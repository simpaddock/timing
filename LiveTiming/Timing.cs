using System;
using rF2SMMonitor;
using rF2SMMonitor.rFactor2Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using static rF2SMMonitor.rFactor2Constants;
using Nancy;
using Nancy.Hosting.Self;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LiveTiming
{
    public class Timing : NancyModule
    {
        MappedBuffer<rF2Telemetry> telemetryBuffer = new MappedBuffer<rF2Telemetry>(rFactor2Constants.MM_TELEMETRY_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Scoring> scoringBuffer = new MappedBuffer<rF2Scoring>(rFactor2Constants.MM_SCORING_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Rules> rulesBuffer = new MappedBuffer<rF2Rules>(rFactor2Constants.MM_RULES_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Extended> extendedBuffer = new MappedBuffer<rF2Extended>(rFactor2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);


        rF2Telemetry telemetry = new rF2Telemetry();
        rF2Scoring scoring = new rF2Scoring();
        rF2Rules rules = new rF2Rules();
        rF2Extended extended = new rF2Extended();
        JArray entries;
        JObject parsedJSON;
        public Timing()
        {
            this.telemetryBuffer.Connect();
            this.scoringBuffer.Connect();
            this.rulesBuffer.Connect();
            this.extendedBuffer.Connect();
            var serializer = new JavaScriptSerializer(); //using System.Web.Script.Serialization;

            String raw = this.GetEntries("http://localhost:8000/api/entries/2");
            parsedJSON = JObject.Parse(raw);
            entries = (JArray)parsedJSON["entries"];


            Options["/{catchAll*}"] = parameters =>
            {
                return new Response { StatusCode = Nancy.HttpStatusCode.Accepted };
            };

            After += (Context) =>
            {
                Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Context.Response.Headers.Add("Access-Control-Allow-Methods", "PUT, GET, POST, DELETE, OPTIONS");
                Context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, x-requested-with, Authorization, Accept, Origin");
            };
            Get["/"] = _ => { Console.WriteLine("jfklas"); return new JavaScriptSerializer().Serialize(this.getData()); };
        }
        ~Timing()
        {
            this.telemetryBuffer.Disconnect();
            this.scoringBuffer.Disconnect();
            this.rulesBuffer.Disconnect();
            this.extendedBuffer.Disconnect();
        }
        public string GetEntries(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        ApiResponse getData()
        {
            ApiResponse res = new ApiResponse();
            Console.Clear();
            this.extendedBuffer.GetMappedData(ref extended);
            this.scoringBuffer.GetMappedData(ref scoring);
            this.telemetryBuffer.GetMappedData(ref telemetry);
            this.rulesBuffer.GetMappedData(ref rules);
            Console.WriteLine("Flag {0}", scoring.mScoringInfo.mYellowFlagState);
            //endet
            res.Session = new LiveTiming.Session
            {
                MaxLaps = scoring.mScoringInfo.mMaxLaps,
                MaxTime = scoring.mScoringInfo.mEndET,
                CurrentTime = scoring.mScoringInfo.mCurrentET,
                CurrentLaps = 0
            };

            foreach (sbyte flag in scoring.mScoringInfo.mSectorFlag)
            {
                rF2YellowFlagState state = (rF2YellowFlagState)flag;

                Console.WriteLine("Flag {0}", (rF2YellowFlagState)flag);

            }
            List<Entry> entries = new List<Entry>();
            for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
            {
                rF2VehicleScoring vehicle = scoring.mVehicles[i];
                rF2VehicleTelemetry vehicleTelementry = telemetry.mVehicles[i];
                rF2TrackRulesParticipant participant = rules.mParticipants[i];


                String[] nameParts = this.GetStringFromBytes(vehicle.mDriverName).Split(' ');

                String vehicleName = this.GetStringFromBytes(vehicleTelementry.mVehicleName);
                String[] vehicleNameParts = vehicleName.Split('#');
                int number = 999;
                try
                {
                    number = Convert.ToInt32(vehicleNameParts[1]);
                }
                catch (Exception)
                {

                }
                String teamName = this.GetStringFromBytes(vehicle.mPitGroup);
                String format = "{0}";

                foreach(JObject driverEntry in this.entries.Children())
                {
                    if (driverEntry["driverNumber"].ToString() == number.ToString())
                    {
                        teamName = driverEntry["teamName"].ToString();
                        format = driverEntry["driverNumberFormat"].ToString();
                    }
                }
                Entry entry = new Entry
                {
                    TeamName = teamName,
                    VehicleName = vehicleNameParts.Length > 1 ? vehicleNameParts[0] : vehicleName,
                    NumberFormat = format,
                    Number = number,
                    FirstName = nameParts.Length > 1 ? nameParts[0] : "",
                    LastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0],
                    Position = vehicle.mPlace,
                    EntryClass = this.GetStringFromBytes(vehicle.mVehicleClass),
                    FrontTires = this.GetStringFromBytes(vehicleTelementry.mFrontTireCompoundName),
                    RearTires = this.GetStringFromBytes(vehicleTelementry.mRearTireCompoundName),
                    PitState = Constants.PitStates[vehicle.mPitState],
                    TimeBehind = vehicle.mTimeBehindNext,
                    LapsBehind = vehicle.mLapsBehindNext,
                    Stops = vehicle.mNumPitstops,
                    Status = Constants.Status[vehicle.mFinishStatus],
                    PositionDifference = vehicle.mQualification - vehicle.mPlace,
                    LastSectorTimes = new double[]
                    {
                        vehicle.mLastSector1,
                        vehicle.mLastSector2,
                        vehicle.mLastLapTime - vehicle.mLastSector2
                    },
                    BestSectorTimes = new double[]
                    {
                        vehicle.mBestSector1,
                        vehicle.mBestSector2,
                        vehicle.mBestLapTime - vehicle.mBestSector2
                    },
                    BestLap = vehicle.mBestLapTime,
                    LastLap = vehicle.mLastLapTime,
                    Laps = vehicle.mTotalLaps
                };
                if (entry.Position == 1)
                {
                    res.Session.CurrentLaps = entry.Laps;
                }
                entries.Add(entry);
            }
            res.Drivers = entries.ToArray();
            res.RaceOverlayControlSet = this.parsedJSON["controlSet"].ToString();
            return res;
        }
        private string GetStringFromBytes(byte[] bytes)
        {
            if (bytes == null)
                return "";

            var nullIdx = Array.IndexOf(bytes, (byte)0);

            return nullIdx >= 0
              ? Encoding.Default.GetString(bytes, 0, nullIdx)
              : Encoding.Default.GetString(bytes);
        }
    }
}
