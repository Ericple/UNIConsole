using System;
using FSUIPC;

namespace UNIConsole.DataSet
{
    [Serializable]
    public class InstrumentDataInfo
    {
        public double IndicatedAirSpeed { get; set; }
        public double BarberPoleAirSpeed { get; set; }
        public double VerticalSpeed { get; set; }
        public FsFrequencyCOM Com1Frequency { get; set; }
        public FsTransponderCode Transponder { get; set; }
        public double MachSpeed { get; set; }
        public ushort Altimeter { get; set; }
        public double Engine1N1 { get; set; }
        public double Engine2N1 { get; set; }
        public double Engine1N2 { get; set; }
        public double Engine2N2 { get; set; }
        public ushort Engine3N1 { get; set; }
        public ushort Engine3N2 { get; set; }
        public ushort Engine4N1 { get; set; }
        public ushort Engine4N2 { get; set; }
        public double RadioAltitude { get; set; }
    }
}
