using System.Collections.Generic;

namespace UNIConsole.DataSet
{
    internal class FlightLog
    {
        public List<FlightLogItem> Logs = new List<FlightLogItem>();
        public List<TouchDownData> TouchDownData = new List<TouchDownData>();
        public double FuelWeight;
        public double FuelConsumption;
        public int AirHours;
        public int AirMinutes;
        public int AirSeconds;
        public int TaxiHours;
        public int TaxiMinutes;
        public int TaxiSeconds;
        public int FlightHours;
        public int FlightMinutes;
        public int FlightSeconds;
        public double PayloadWeight;
        public int SETHours;
        public int SETMinutes;
        public int SETSeconds;
        public double LandingLightScore;
        public int Score=1000;
    }
}
