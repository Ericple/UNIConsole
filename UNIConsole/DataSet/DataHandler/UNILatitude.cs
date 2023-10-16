using FSUIPC;

namespace UNIConsole.DataSet.DataHandler
{
    public class UNILatitude
    {
        public long OriginValue;
        public double DecimalValue;
        public string DMSValue;
        public UNILatitude(long value)
        {
            var temp = new FsLatitude(value);
            DecimalValue = temp.DecimalDegrees + temp.DecimalMinutes / 60 + temp.DecimalSeconds / 3600;
            DMSValue = temp.ToString();
        }
    }
}
