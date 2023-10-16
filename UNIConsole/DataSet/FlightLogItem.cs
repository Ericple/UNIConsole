using System;
using System.Text;

namespace UNIConsole.DataSet
{
    public enum FlightLogItemType
    {
        Normal, Good, Bad
    }
    [Serializable]
    public class FlightLogItem
    {
        public FlightLogItemType Type;
        public AircraftDataInfo AcDataLog { get; set; }
        public InstrumentDataInfo InsDataLog { get; set; }
        public string LogContent { get; set; }
        public int ScoreAdjust { get; set; }
        public DateTime Time = DateTime.UtcNow;
        public FlightLogItem(FlightLogItemType type, string logContent, int scoreAdjust, AircraftData acDataLog, InstrumentData insDataLog, bool logToConsole = false)
        {
            Type = type;
            AcDataLog = (AircraftDataInfo)acDataLog.ToInfo();
            InsDataLog = (InstrumentDataInfo)insDataLog.ToInfo();
            LogContent = logContent;
            ScoreAdjust = scoreAdjust;
            ServiceServer.SendBytesOverIpc(Encoding.UTF8.GetBytes($"log*[{DateTime.UtcNow}] {logContent}"), logToConsole);
        }
    }
}
