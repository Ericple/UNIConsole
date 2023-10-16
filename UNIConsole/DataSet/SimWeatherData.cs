using FSUIPC;
using UNIConsole.Helper;

namespace UNIConsole.DataSet
{
    internal class SimWeatherData : DataSetBase
    {
        public ushort CurrentVisibility { get; set; }
        public ushort OutsideAirTemperature { get; set; }
        public ushort AmbientWindSpeed { get; set; }
        public ushort AmbientWindDirection { get; set; }
        public static string GroupName { get; set; } = "SimWeatherData";
        public Offset[] Offsets { get; set; } = {
            new Offset<ushort>(GroupName, 0x0E8A),
            new Offset<ushort>(GroupName, 0x0E8C),
            new Offset<ushort>(GroupName, 0x0E90),
            new Offset<ushort>(GroupName, 0x0E92),
        };
        public SimWeatherData()
        {
            Initialize();
        }
        public override object ToInfo()
        {
            return new SimWxDataInfo
            {
                CurrentVisibility = CurrentVisibility / 1000d,
                OutsideAirTemperature = ValueHelper.Temperature(OutsideAirTemperature),
                AmbientWindSpeed = AmbientWindSpeed,
                AmbientWindDirection = ValueHelper.WindSpdDir(AmbientWindDirection),
            };
        }
    }
}
