using FSUIPC;
using UNIConsole.Helper;

namespace UNIConsole.DataSet
{
    internal class SimulatorData : DataSetBase
    {
        public byte ZuluHour { get; set; }
        public byte ZuluMinute { get; set; }
        public byte ZuluSecond { get; set; }
        public ushort PauseIndicator { get; set; }
        public ushort FrameRate { get; set; }
        public ushort SimulationRate { get; set; }
        public ushort FSVersion { get; set; }
        public string SimPath { get; set; }
        public ushort WindTurbulence { get; set; }
        public static string GroupName { get; set; } = "SimulatorData";
        public Offset[] Offsets { get; set; } = {
            new Offset<byte>(GroupName, 0x023B),
            new Offset<byte>(GroupName, 0x023C),
            new Offset<byte>(GroupName, 0x023A),
            new Offset<ushort>(GroupName, 0x0264),
            new Offset<ushort>(GroupName, 0x0274),
            new Offset<ushort>(GroupName, 0x0C1A),
            new Offset<ushort>(GroupName, 0x3308),
            new Offset<string>(GroupName, 0x3E00, 256),
            new Offset<ushort>(GroupName, 0x0E98)
        };
        public SimulatorData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new SimulatorDataInfo
            {
                ZuluHour = ZuluHour,
                ZuluMinute = ZuluMinute,
                ZuluSecond = ZuluSecond,
                PauseIndicator = PauseIndicator != 0,
                FrameRate = ValueHelper.FrameRate(FrameRate),
                SimulationRate = ValueHelper.SimulationRate(SimulationRate),
                TurblencePrecentage = 100 * WindTurbulence / 255
            };
        }
    }
}
