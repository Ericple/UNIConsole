using FSUIPC;
using UNIConsole.Helper;

namespace UNIConsole.DataSet
{
    internal class FlightControlData : DataSetBase
    {
        public int AutoThrottleToga { get; set; }
        public int AutoThrottleArm { get; set; }
        public ushort ParkingBrake { get; set; }
        public int SpoilersArm { get; set; }
        public int FlapsPL { get; set; }
        public int FlapsPR { get; set; }
        public int GearControl { get; set; }
        public int MasterAvionics { get; set; }
        public byte AutoBrakeSwitch { get; set; }
        public int AutoPilot { get; set; }
        public int MasterBattery { get; set; }
        public short Engine1Lever { get; set; }
        public ushort Engine1Starter { get; set; }
        public ushort Engine1AntiIce { get; set; }
        public short Engine2Lever { get; set; }
        public ushort Engine2Starter { get; set; }
        public ushort Engine2AntiIce { get; set; }
        public short ElevatorCI { get; set; }
        public short AileronCI { get; set; }
        public short RudderCI { get; set; }
        public short ElevatorTCI { get; set; }
        public short ElevatorPI { get; set; }
        public short AileronPI { get; set; }
        public short RudderPI { get; set; }
        public short ElevatorTI { get; set; }
        public double AileronTAI { get; set; }
        public double RudderTAI { get; set; }
        public int AutoPilotMasterSwitch { get; set; }
        public static string GroupName { get; set; } = "FlightControlData";
        public Offset[] Offsets { get; set; } = {
            new Offset<int>(GroupName, 0x080C),
            new Offset<int>(GroupName, 0x0810),
            new Offset<ushort>(GroupName, 0x0BC8),
            new Offset<int>(GroupName, 0x0BCC),
            new Offset<int>(GroupName, 0x0BE0),
            new Offset<int>(GroupName, 0x0BE4),
            new Offset<int>(GroupName, 0x0BE8),
            new Offset<int>(GroupName, 0x2E80),
            new Offset<byte>(GroupName, 0x2F80),
            new Offset<int>(GroupName, 0x07BC),
            new Offset<int>(GroupName, 0x281C),
            new Offset<short>(GroupName, 0x088C),
            new Offset<ushort>(GroupName, 0x0892),
            new Offset<ushort>(GroupName, 0x08B2),
            new Offset<short>(GroupName, 0x0924),
            new Offset<ushort>(GroupName, 0x092A),
            new Offset<ushort>(GroupName, 0x094A),
            new Offset<short>(GroupName, 0x0BB2),
            new Offset<short>(GroupName, 0x0BB6),
            new Offset<short>(GroupName, 0x0BBA),
            new Offset<short>(GroupName, 0x0BC0),
            new Offset<short>(GroupName, 0x0BB4),
            new Offset<short>(GroupName, 0x0BB8),
            new Offset<short>(GroupName, 0x0BBC),
            new Offset<short>(GroupName, 0x0BC2),
            new Offset<double>(GroupName, 0x0480),
            new Offset<double>(GroupName, 0x0488),
        };
        public FlightControlData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new FlightControlDataInfo
            {
                AutoThrottleToga = AutoThrottleToga != 0,
                AutoThrottleArm = AutoThrottleArm != 0,
                ParkingBrake = ValueHelper.ParkingBrake(ParkingBrake),
                SpoilersArm = SpoilersArm != 0,
                FlapsPL = ValueHelper.FlapP(FlapsPL),
                FlapsPR = ValueHelper.FlapP(FlapsPR),
                GearControl = GearControl != 0,
                MasterAvionics = MasterAvionics != 0,
                AutoBrakeSwitch = AutoBrakeSwitch,
                AutoPilot = AutoPilot != 0,
                MasterBattery = MasterBattery != 0,
                Engine1Lever = ValueHelper.Engine(Engine1Lever),
                Engine1Starter = Engine1Starter,
                Engine1AntiIce = Engine1AntiIce,
                Engine2Lever = ValueHelper.Engine(Engine2Lever),
                Engine2Starter = Engine2Starter,
                Engine2AntiIce = Engine2AntiIce,
                ElevatorCI = ValueHelper.ControlSurface(ElevatorCI),
                AileronCI = ValueHelper.ControlSurface(AileronCI),
                RudderCI = ValueHelper.ControlSurface(RudderCI),
                ElevatorTCI = ValueHelper.ControlSurface(ElevatorTCI),
                ElevatorPI = ValueHelper.ControlSurface(ElevatorPI),
                AileronPI = ValueHelper.ControlSurface(AileronPI),
                RudderPI = ValueHelper.ControlSurface(RudderPI),
                ElevatorTI = ValueHelper.ControlSurface(ElevatorTI),
                AileronTAI = ValueHelper.ControlSurface(AileronTAI),
                RudderTAI = ValueHelper.ControlSurface(RudderTAI)
            };
        }
    }
}
