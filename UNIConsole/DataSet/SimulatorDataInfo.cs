using System;

namespace UNIConsole.DataSet
{
    [Serializable]
    internal class SimulatorDataInfo
    {
        public byte ZuluHour { get; set; }
        public byte ZuluMinute { get; set; }
        public byte ZuluSecond { get; set; }
        public bool PauseIndicator { get; set; }
        public int FrameRate { get; set; }
        public int SimulationRate { get; set; }
        public double TurblencePrecentage { get; set; }
    }
}
