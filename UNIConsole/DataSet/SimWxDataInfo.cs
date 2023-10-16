using System;

namespace UNIConsole.DataSet
{
    [Serializable]
    internal class SimWxDataInfo
    {
        public double CurrentVisibility { get; set; }
        public double OutsideAirTemperature { get; set; }
        public ushort AmbientWindSpeed { get; set; }
        public double AmbientWindDirection { get; set; }
    }
}
