using System;

namespace UNIConsole.DataSet
{
    [Serializable]
    internal class WarningDataInfo
    {
        public bool StallWarning { get; set; }
        public bool OverSpeedWarning { get; set; }
    }
}
