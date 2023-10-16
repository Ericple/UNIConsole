using System;

namespace UNIConsole.DataSet
{
    [Serializable]
    public class LightsDataInfo
    {
        public bool Navigation { get; set; }
        public bool Beacon { get; set; }
        public bool Landing { get; set; }
        public bool Taxi { get; set; }
        public bool Strobes { get; set; }
        public bool Instruments { get; set; }
        public bool Recognition { get; set; }
        public bool Wing { get; set; }
        public bool Logo { get; set; }
        public bool Cabin { get; set; }
    }
}
