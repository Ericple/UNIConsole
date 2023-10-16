using FSUIPC;
using UNIConsole.Helper;

namespace UNIConsole.DataSet
{
    public class InstrumentData : DataSetBase
    {
        public uint IndicatedAirSpeed { get; set; }
        public uint BarberPoleAirSpeed { get; set; }
        public int VerticalSpeed { get; set; }
        public ushort Com1Frequency { get; set; }
        public ushort Transponder { get; set;}
        public ushort MachSpeed { get; set; }
        public ushort Altimeter { get; set; }
        public ushort Engine1N1 { get; set; }
        public ushort Engine2N1 { get; set; }
        public ushort Engine1N2 { get; set; }
        public ushort Engine2N2 { get; set; }
        public ushort Engine3N1 { get; set; }
        public ushort Engine3N2 { get; set; }
        public ushort Engine4N1 { get; set; }
        public ushort Engine4N2 { get; set; }
        public int RadioAltitude { get; set; }
        public static string GroupName { get; set; } = "InstrumentData";
        public Offset[] Offsets { get; set; } = {
            new Offset<uint>(GroupName, 0x02BC),
            new Offset<uint>(GroupName, 0x02C4),
            new Offset<int>(GroupName, 0x02C8),
            new Offset<ushort>(GroupName, 0x034E),
            new Offset<ushort>(GroupName, 0x0354),
            new Offset<ushort>(GroupName, 0x11C6),
            new Offset<ushort>(GroupName, 0x0330),
            new Offset<ushort>(GroupName, 0x0898),
            new Offset<ushort>(GroupName, 0x0930),
            new Offset<ushort>(GroupName, 0x0896),
            new Offset<ushort>(GroupName, 0x092E),
            new Offset<ushort>(GroupName, 0x09C8),
            new Offset<ushort>(GroupName, 0x09C6),
            new Offset<ushort>(GroupName, 0x0A60),
            new Offset<ushort>(GroupName, 0x0A5E),
            new Offset<int>(GroupName, 0x31E4)
        };
        public InstrumentData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new InstrumentDataInfo
            {
                IndicatedAirSpeed = ValueHelper.AirSpeed(IndicatedAirSpeed),
                BarberPoleAirSpeed = ValueHelper.AirSpeed(BarberPoleAirSpeed),
                VerticalSpeed = ValueHelper.VSFPM(VerticalSpeed),
                Com1Frequency = new FsFrequencyCOM(Com1Frequency),
                Transponder = new FsTransponderCode(Transponder),
                MachSpeed = ValueHelper.MachSpeed(MachSpeed),
                Altimeter = Altimeter,
                Engine1N1 = ValueHelper.Engine(Engine1N1),
                Engine2N1 = ValueHelper.Engine(Engine2N1),
                Engine1N2 = ValueHelper.Engine(Engine1N2),
                Engine2N2 = ValueHelper.Engine(Engine2N2),
                RadioAltitude = ValueHelper.RadioAltitude(RadioAltitude),
            };
        }
    }
}
