using FSUIPC;
using UNIConsole.DataSet.DataHandler;
using UNIConsole.Helper;

namespace UNIConsole.DataSet
{
    public class AircraftData : DataSetBase
    {
        public int GroundAltitude { get; set; }
        public uint GroundSpeed { get; set; }
        public uint AirSpeed { get; set; }
        public double Heading { get; set; }
        public long Latitude { get; set; }
        public long Longitude { get; set; }
        public int Pitch { get; set; }
        public int Roll { get; set; }
        public ushort EngineNumber { get; set; }
        public ushort CrashFlag { get; set; }
        public string AircraftType { get; set; }
        public int ZeroFuelWeight { get; set; }
        public ushort OnGround { get; set; }
        public short GForce { get; set; }
        public ushort Engine1EGT { get; set; }
        public ushort Engine2EGT { get; set; }
        public int GearPN { get; set; }
        public int GearPR { get; set; }
        public int GearPL { get; set; }
        public short GForceOnTouchDown { get; set; }
        public ushort Engine1Firing { get; set; }
        public ushort Engine2Firing { get; set; }
        public long Altitude { get; set; }
        public static string GroupName { get; set; } = "AircraftData";
        public Offset[] Offsets { get; set; } = {
                new Offset<int>(GroupName, 0x0020),
                new Offset<uint>(GroupName, 0x02B4),
                new Offset<uint>(GroupName, 0x02B8),
                new Offset<double>(GroupName, 0x02CC),
                new Offset<long>(GroupName, 0x0560),
                new Offset<long>(GroupName, 0x0568),
                new Offset<int>(GroupName, 0x0578),
                new Offset<int>(GroupName, 0x057C),
                new Offset<ushort>(GroupName, 0x0840),
                new Offset<ushort>(GroupName, 0x0AEC),
                //new Offset<string>(GroupName, 0x3160, 23),
                new Offset<string>(GroupName, 0x3D00, 256),
                new Offset<int>(GroupName, 0x3BFC),
                new Offset<ushort>(GroupName, 0x0366),
                new Offset<short>(GroupName, 0x11BA),
                new Offset<ushort>(GroupName, 0x08BE),
                new Offset<ushort>(GroupName, 0x0956),
                new Offset<int>(GroupName, 0x0BEC),
                new Offset<int>(GroupName, 0x0BF0),
                new Offset<int>(GroupName, 0x0BF4),
                new Offset<short>(GroupName, 0x11B8),
                new Offset<ushort>(GroupName, 0x0894),
                new Offset<ushort>(GroupName, 0x098C),
                new Offset<long>(GroupName, 0x0570),
        };
        public AircraftData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new AircraftDataInfo
            {
                GroundAltitude = new UNIGroundAltitude(GroundAltitude),
                GroundSpeed = new UNIGroundSpeed(GroundSpeed),
                AirSpeed = new UNIAirSpeed(AirSpeed),
                Heading = Heading,
                Latitude = new UNILatitude(Latitude),
                Longitude = new UNILongitude(Longitude),
                Pitch = ValueHelper.PitchRoll(Pitch),
                Roll = ValueHelper.PitchRoll(Roll),
                EngineNumber = EngineNumber,
                CrashFlag = CrashFlag,
                AircraftType = AircraftType,
                ZeroFuelWeight = ValueHelper.ZFW(ZeroFuelWeight),
                OnGround = OnGround,
                GForce = ValueHelper.GFTD(GForce),
                Engine1EGT = ValueHelper.Engine(Engine1EGT),
                Engine2EGT = ValueHelper.Engine(Engine2EGT),
                GearPN = ValueHelper.GearP(GearPN),
                GearPR = ValueHelper.GearP(GearPR),
                GearPL = ValueHelper.GearP(GearPL),
                GForceOnTouchDown = ValueHelper.GFTD(GForceOnTouchDown),
                Engine1Firing = Engine1Firing,
                Engine2Firing = Engine2Firing,
                Altitude = new UNIAltitude(Altitude)
            };
        }
        public void SetGTD(short GFTD)
        {
            Offsets[19].SetValue(GFTD);
        }
    }
}
