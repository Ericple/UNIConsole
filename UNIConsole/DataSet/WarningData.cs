using FSUIPC;

namespace UNIConsole.DataSet
{
    internal class WarningData : DataSetBase
    {
        public byte StallWarning { get; set; }
        public byte OverSpeedWarning { get; set; }
        public static string GroupName { get; set; } = "WarningData";
        public Offset[] Offsets { get; set; } = {
            new Offset<byte>(GroupName, 0x036C),
            new Offset<byte>(GroupName, 0x036D),
        };
        public WarningData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new WarningDataInfo
            {
                StallWarning = StallWarning != 0,
                OverSpeedWarning = OverSpeedWarning != 0,
            };
        }
    }
}
