using System;

namespace UNIConsole.DataSet
{
    [Serializable]
    public class FlightControlDataInfo
    {
        public bool PilotHeat { get; set; }
        public bool AutoThrottleToga { get; set; }
        public bool AutoThrottleArm { get; set; }
        public double ParkingBrake { get; set; }
        public bool SpoilersArm { get; set; }
        public double FlapsPL { get; set; }
        public double FlapsPR { get; set; }
        public bool GearControl { get; set; }
        public bool MasterAvionics { get; set; }
        public byte AutoBrakeSwitch { get; set; }
        public bool AutoPilot { get; set; }
        public bool MasterBattery { get; set; }
        public double Engine1Lever { get; set; }
        public ushort Engine1Starter { get; set; }
        public ushort Engine1AntiIce { get; set; }
        public double Engine2Lever { get; set; }
        public ushort Engine2Starter { get; set; }
        public ushort Engine2AntiIce { get; set; }
        public double ElevatorCI { get; set; }
        public double AileronCI { get; set; }
        public double RudderCI { get; set; }
        public double ElevatorTCI { get; set; }
        public double ElevatorPI { get; set; }
        public double AileronPI { get; set; }
        public double RudderPI { get; set; }
        public double ElevatorTI { get; set; }
        public double AileronTAI { get; set; }
        public double RudderTAI { get; set; }
    }
}
