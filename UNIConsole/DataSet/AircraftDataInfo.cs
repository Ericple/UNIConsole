using System;
using UNIConsole.DataSet.DataHandler;

namespace UNIConsole.DataSet
{
    [Serializable]
    public class AircraftDataInfo
    {
        public UNIGroundAltitude GroundAltitude { get; set; }
        public UNIGroundSpeed GroundSpeed { get; set; }
        public UNIAirSpeed AirSpeed { get; set; }
        public double Heading { get; set; }
        public UNILatitude Latitude { get; set; }
        public UNILongitude Longitude { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public ushort EngineNumber { get; set; }
        public ushort CrashFlag { get; set; }
        public string AircraftType { get; set; }
        public double ZeroFuelWeight { get; set; }
        public ushort OnGround { get; set; }
        public double GForce { get; set; }
        public double Engine1EGT { get; set; }
        public double Engine2EGT { get; set; }
        public double GearPN { get; set; }
        public double GearPR { get; set; }
        public double GearPL { get; set; }
        public double GForceOnTouchDown { get; set; }
        public ushort Engine1Firing { get; set; }
        public ushort Engine2Firing { get; set; }
        public UNIAltitude Altitude { get; set; }
    }
}
