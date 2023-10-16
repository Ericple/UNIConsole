using FSUIPC;

namespace UNIConsole.DataSet.DataHandler
{
    public class UNILongitude
    {
        public long OriginValue;
        public double DecimalValue;
        public string DMSValue;
        public UNILongitude(long value)
        {
            var temp = new FsLongitude(value);
            DecimalValue = temp.DecimalDegrees + temp.DecimalMinutes / 60 + temp.DecimalSeconds / 3600;
            DMSValue = temp.ToString();
        } 
    }
}
