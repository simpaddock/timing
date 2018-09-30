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
        JArray entries;
        JObject parsedJSON;
        public Timing()
        { 

            var serializer = new JavaScriptSerializer(); //using System.Web.Script.Serialization;

            String raw = this.GetEntries(Constants.PADDOCKURL);
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
            Get["/"] = _ => {
                return new JavaScriptSerializer().Serialize(this.getData()); };
        }
        public string GetEntries(string uri)
        {
            try
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
            catch
            {
                return "";
            }
        }

        ApiResponse getData()
        {
            ApiResponse res = new ApiResponse();
            Console.Clear();
            Program.extendedBuffer.GetMappedData(ref Program.extended);
            Program.scoringBuffer.GetMappedData(ref Program.scoring);
            Program.telemetryBuffer.GetMappedData(ref Program.telemetry);
            Program.rulesBuffer.GetMappedData(ref Program.rules);

            var scoring = Program.scoring;
            var telemetry = Program.telemetry;
            var rules = Program.rules;
            res.Session = new LiveTiming.Session
            {
                MaxLaps = scoring.mScoringInfo.mMaxLaps,
                MaxTime = scoring.mScoringInfo.mEndET - Constants.MAXTIMECOUNTDOWN, // I assume that the seconds are just becaus of the red light countdown until the lights go on
                CurrentTime = Math.Floor(scoring.mScoringInfo.mCurrentET),
                CurrentLaps = 0,
                YellowFlags = new bool[3],
                IsRace = scoring.mScoringInfo.mSession >= 10 && scoring.mScoringInfo.mSession <= 13,
                IsSessionStarted = scoring.mScoringInfo.mGamePhase == 5,
                IsVCY = scoring.mScoringInfo.mGamePhase == 6
            };
            for (int i = 0; i < 3; i++)
            {
                int raw = Convert.ToInt32(scoring.mScoringInfo.mSectorFlag[i]);
                res.Session.YellowFlags[i] = raw != 11;
            }
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
                dynamic foo = vehicle.mTrackEdge; //??
                Console.WriteLine(foo);
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

                TimeSpan diff = TimeSpan.FromSeconds(vehicle.mTimeBehindNext);
                String diffString = diff.ToString();
                String bestLapString = TimeSpan.FromSeconds(vehicle.mBestLapTime).ToString(@"mm\:ss\:fff");
                String lastLapString = TimeSpan.FromSeconds(vehicle.mLastLapTime).ToString(@"mm\:ss\:fff");

                if (diff.TotalHours < 1)
                {
                    if (diff.TotalMinutes < 1)
                    {
                        diffString = diff.ToString(@"ss\:fff");
                    } else
                    {
                        diffString = diff.ToString(@"mm\:ss\:fff");
                    }
                }

                diffString = String.Format("+ {0}", diffString);
                int positionDifference = Program.lastResponse != null ? vehicle.mPlace - Program.lastResponse.Drivers.First(d => d.SlotID == vehicle.mID).Position : 0;
                Console.Write(positionDifference);
                Entry entry = new Entry
                {
                    SlotID = vehicle.mID,
                    TeamName = teamName,
                    VehicleName = vehicleNameParts.Length > 1 ? vehicleNameParts[0] : vehicleName,
                    NumberFormat = format,
                    Number = number,
                    FormattedNumber = String.Format(format, number),
                    FirstName = nameParts.Length > 1 ? nameParts[0] : "",
                    LastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0],
                    Position = vehicle.mPlace,
                    EntryClass = this.GetStringFromBytes(vehicle.mVehicleClass),
                    FrontTires = this.GetStringFromBytes(vehicleTelementry.mFrontTireCompoundName),
                    RearTires = this.GetStringFromBytes(vehicleTelementry.mRearTireCompoundName),
                    PitState = Constants.PitStates[vehicle.mPitState],
                    TimeBehind = vehicle.mTimeBehindNext,
                    TimeBehindString = diffString,
                    LapsBehind = vehicle.mLapsBehindNext,
                    Stops = vehicle.mNumPitstops,
                    Status = Constants.Status[vehicle.mFinishStatus],
                    HasHeatingProblem = vehicleTelementry.mOverheating != 0,
                    HasLostParts = vehicleTelementry.mDetached !=0,
                    CurrentLapString = TimeSpan.FromSeconds(res.Session.CurrentTime - vehicleTelementry.mLapStartET).ToString(@"mm\:ss\:fff"),
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
                    LastLapString = lastLapString,
                    BestLapString = bestLapString,
                    Laps = vehicle.mTotalLaps,
                    CurrentSessionPositionDifference = positionDifference
                };
                if (entry.Position == 1)
                {
                    res.Session.CurrentLaps = entry.Laps;
                }
                entries.Add(entry);
            }
            // Set fastest lap
            Entry fastestDriver = entries.First(d => d.BestLap == entries.Min(e => e.BestLap));
            entries.ForEach(e =>
            {
                e.BestLapDelta = e.BestLap - fastestDriver.BestLap;
                TimeSpan diff = TimeSpan.FromSeconds(e.BestLapDelta);
                if (diff.TotalMinutes < 1.0)
                {
                    e.BestLapDeltaString = "+ " + diff.ToString(@"ss\:fff");
                }
                else
                {
                    e.BestLapDeltaString = "+ " + diff.ToString(@"mm\:ss\:fff");
                }
            });

            if (res.Session.MaxLaps == int.MaxValue)
            {
                TimeSpan current = TimeSpan.FromSeconds(Math.Floor(scoring.mScoringInfo.mCurrentET));
                TimeSpan max = TimeSpan.FromSeconds(Math.Floor(scoring.mScoringInfo.mEndET - Constants.MAXTIMECOUNTDOWN));
                res.Session.SessionLeftString = String.Format("{0}/ {1}", current, max);
            }
            else
            {
                res.Session.CurrentLaps = entries.Max(e => e.Laps);
                res.Session.SessionLeftString = String.Format("{0}/ {1}", res.Session.CurrentLaps, res.Session.MaxLaps);
            }
            res.Drivers = entries.ToArray();
            res.RaceOverlayControlSet = this.parsedJSON["controlSet"].ToString();
            res.CommandId = Convert.ToInt32(this.parsedJSON["commandId"]);
            res.Session.IsSessionPaused = res.RaceOverlayControlSet.IndexOf("pause") != -1;
            // Translate the slot id (wich means the position) to the proper rfactor 2 id:
            bool driverFound = false;
            if (res.RaceOverlayControlSet.IndexOf("currentDriver") != -1)
            {
                String driverName = JObject.Parse(res.RaceOverlayControlSet)["currentDriver"].ToString();
                foreach (Entry driver in res.Drivers)
                {
                    if (driver.FirstName + " " + driver.LastName == driverName )
                    {
                        res.SlotId = driver.SlotID;
                        driverFound = true;
                        break;
                    }
                }
            } else
            {
                foreach (Entry driver in res.Drivers)
                {
                    if (driver.Position == Convert.ToInt32(this.parsedJSON["slotId"].ToString()))
                    {
                        res.SlotId = driver.SlotID;
                        driverFound = true;
                        break;
                    }
                }
            }

            res.SlotId = Program.lastResponse != null && res.SlotId == 0 ? Program.lastResponse.SlotId : res.SlotId;
            res.CameraId = Convert.ToInt32(this.parsedJSON["cameraId"].ToString());
            if (!driverFound && Program.lastResponse != null)
            {
                res.SlotId = Program.lastResponse.SlotId;
                res.CameraId = Program.lastResponse.CameraId;
                res.CommandId = Program.lastResponse.CameraId;
            }
            // Write control file for rfactor plugin
            // TODO: ADD A PROPER TIMEOUT
            try
            {
                if (Program.lastResponse == null || Program.lastResponse.CameraId != res.CameraId || Program.lastResponse.SlotId != res.SlotId)
                {
                    string[] lines = { res.SlotId.ToString(), res.CameraId.ToString() };
                    String path = Path.Combine(Path.GetTempPath(), "cameraslots.txt");
                    System.IO.File.WriteAllLines(path, lines);
                }
            }
            catch
            {

            }
            Program.lastResponse = res;
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
